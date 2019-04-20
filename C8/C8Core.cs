using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace C8
{
    class C8Core
    {
        // Constants
        const int G_WIDTH = 64;
        const int G_HEIGHT = 32;

        // Default game path
        public string gamePath = "";

        // Objects
        public C8Graphics display;                     // Graphics object
        public uint[] key = new uint[16];              // Array to store key presses
        Random rnd = new Random();                     // Random object

        // Using uint for the registers as it makes it easier to use the bitwise operators
        private uint opcode;                            // The current opcode
        private uint PC;                                // Program Counter
        private uint I;                                 // Address register
        private uint[] V = new uint[16];                // V0 to VE registers and VF flag
        private Stack<uint> stack = new Stack<uint>();  // Stack (should be 24 levels)
        private uint timd, tims;                        // Delay timer and sound timer
        private uint[] mem = new uint[4096];            // 4K of memory

        // Memory Map
        // 0x000 to 0x1FF - Not used, can be used for font set
        // 0x050 to 0x0A0 - Used to store the built in 4x5 pixel font set
        // 0x200 to 0xFFF - Program ROM and work RAM

        // Key Map
        //
        // Key ID 0 to F              Keyboard Keys
        // -----------------         -----------------
        // | 1 | 2 | 3 | C |         | 1 | 2 | 3 | 4 |
        // | - | - | - | - |         | - | - | - | - |
        // | 4 | 5 | 6 | D |         | Q | U | E | R |
        // | - | - | - | - |         | - | - | - | - |
        // | 7 | 8 | 9 | E |         | A | S | D | F |
        // | - | - | - | - |         | - | - | - | - |
        // | A | 0 | B | F |         | Z | X | C | V |
        // -----------------         -----------------

        public C8Core(PictureBox PB)
        {
            // Create the new display object
            display = new C8Graphics(PB);

            InitRegs();
        }

        /* 
        //Load test data
        public void LoadTest()
        {
            byte[] test_data = {
                0x60, 0x01, // Set v0 to 1
                0x62, 0x02, // Set v2 to 2
                0xF0, 0x29, // Set I to '1' sprite
                0xD1, 0x15, // Draw the 1 at 0,0
                0xD2, 0x15, // Draw the 1 at 2,0
                0x00, 0x00, 
                0x00, 0x00,
                0x00, 0x00
            };
    
                for (int a = 0; a < test_data.Length; a++)
                {
                    mem[0x200 + a] = test_data[a];
                }
        }
        */

        // Load a new ROM
        public void LoadRom()
        {
            FileStream FS = File.Open(gamePath, FileMode.Open);
            byte[] data = new byte[3583];
            UTF8Encoding utf = new UTF8Encoding(true);

            int count;

            do
            {
                count = FS.Read(data, 0, data.Length);
                for (int a = 0; a < data.Length; a++)
                {
                    mem[0x200 + a] = data[a];
                }
            } while (count > 0);
            FS.Close();
        }

        // Main loop called every 16ms
        public void MainLoop()
        {           
            CPUCycle();

            if (tims != 0)
            {
                Console.Beep();
            }
        }

        // Execute a single cycle
        private void CPUCycle()
        {
            // Fetch the opcode (2 bytes)
            opcode = (mem[PC] << 8) + mem[PC + 1];

            //opcode = 0x0ABC;
            Debug.WriteLine("PC= " + PC.ToString("X4"));
            Debug.WriteLine("OP= " + opcode.ToString("X4"));

            // Decode the opcode
            switch (opcode & 0xF000)
            {
                case 0x0000: 
                    switch (opcode & 0x0FFF)
                    {
                        case 0x0000: // 0NNN Call RCA 1802 program at address NNN
                            // Not implemented
                            PC += 2;
                            break;
                        case 0x00E0: // 00E0 Clear the screen
                            display.Clear();
                            PC += 2;
                            break;
                        case 0x00EE: // 00EE Return from subroutine
                            PC = stack.Pop() + 2;
                            break;
                    }
                    break;
                case 0x1000: // 1NNN Jump to address NNN
                     PC = opcode & 0x0FFF;
                     break;
                case 0x2000: // 2NNN Call subroutine at address NNN
                    stack.Push(PC);
                    PC = opcode & 0x0FFF;
                    break;
                case 0x3000: // 3XNN Skip next instruction if VX = NN                    
                    PC = (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))? PC + 4 : PC + 2;
                    break;
                case 0x4000: // 4XNN Skip next instruction if VX != NN                    
                    PC = (V[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF)) ? PC + 4 : PC + 2;
                    break;
                case 0x5000: // 5XY0 Skip next instruction if VX = VY                    
                    PC = (V[(opcode & 0x0F00) >> 8] == V[(opcode & 0x00F0) >> 4]) ? PC + 4 : PC + 2;
                    break;
                case 0x6000: // 6XNN Set VX to NN                   
                    V[(opcode & 0x0F00) >> 8] = opcode & 0x00FF;
                    PC += 2;
                    break;
                case 0x7000: // 7XNN Add NN to VX (Carry flag not changed)                
                    V[(opcode & 0x0F00) >> 8] = (V[(opcode & 0x0F00) >> 8] + (opcode & 0x00FF)) % 0x100;
                    PC += 2;
                    break;
                case 0x8000:
                    uint XREG = (opcode & 0x0F00) >> 8;
                    uint YREG = (opcode & 0x00F0) >> 4;

                    switch (opcode & 0x000F)
                    {
                        case 0x0: // 8XY0 Sets the value of VX to VY                
                            V[XREG] = V[YREG];
                            PC += 2;
                            break;
                        case 0x1: // 8XY1 Sets VX to VX or VY. (Bitwise OR operation)               
                            V[XREG] |= V[YREG];
                            PC += 2;
                            break;
                        case 0x2: // 8XY2 Sets VX to VX & VY. (Bitwise AND operation)               
                            V[XREG] &= V[YREG];
                            PC += 2;
                            break;
                        case 0x3: // 8XY3 Sets VX to VX XOR VY. (Bitwise XOR operation)               
                            V[XREG] ^= V[YREG];
                            PC += 2;
                            break;
                        case 0x4: // 8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't. 
                            V[0xF] = (V[XREG] + V[YREG] > 255) ? (uint)0x1 : 0;
                            V[XREG] = (V[XREG] + V[YREG]) % 0x100;
                            PC += 2;
                            break;
                        case 0x5: // 8XY5 VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            if (V[XREG] >= V[YREG])
                            {
                                V[XREG] = V[XREG] - V[YREG];
                                V[0xF] = (uint)0x1;
                            } else
                            {
                                V[XREG] = V[YREG] - V[XREG];
                                V[0xF] = 0;
                            }                          
                            PC += 2;
                            break;
                        case 0x6: // 8XY6 Stores the least significant bit of VX in VF and then shifts VX to the right by 1 
                            V[0xF] = V[XREG] & 0x01;
                            V[XREG] >>= 1;
                            PC += 2;
                            break;
                        case 0x7: // 8XY7 Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            if (V[YREG] >= V[XREG])
                            {
                                V[XREG] = V[YREG] - V[XREG];
                                V[0xF] = (uint)0x1;
                            }
                            else
                            {
                                V[XREG] = V[XREG] - V[YREG];
                                V[0xF] = 0;
                            }
                            PC += 2;
                            break;
                        case 0xE: // 8XYE Stores the most significant bit of VX in VF and then shifts VX to the left by 1 
                            V[0xF] = V[XREG] & 0x80;
                            V[XREG] = (V[XREG] << 1) %100;
                            PC += 2;
                            break;
                    }
                    break;
                case 0x9000: // 9XY0 Skips the next instruction if VX doesn't equal VY                 
                    PC = (V[(opcode & 0x0F00) >> 8] != V[(opcode & 0x00F0) >> 4]) ? PC + 4 : PC + 2;
                    break;
                case 0xA000: // ANNN Set I to NNN
                    I = opcode & 0x0FFF;
                    PC += 2;
                    break;
                case 0xB000: // BNNN Jump to address NNN + V0
                    PC = (opcode & 0x0FFF) + V[0];
                    break;
                case 0xC000: // CXNN Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                    V[(opcode & 0x0F00) >> 8] = (uint)((opcode & 0x00FF) & rnd.Next(0, 256));
                    PC += 2;
                    break;
                case 0xD000:
                    // DXYN Draw a sprite at VX,YV that has a width of 8px and height of Npx
                    // Each row of 8 pixels is read as bit-coded starting from memory location I; 
                    // I value doesn’t change after the execution of this instruction. 
                    // VF is set to 1 if any screen pixels are flipped from set to unset when the 
                    // sprite is drawn, and to 0 if that doesn’t happen
                    uint X = V[(opcode & 0x0F00) >> 8];
                    uint Y = V[(opcode & 0x00F0) >> 4];
                    uint height = opcode & 0x000F;
                    uint row_data;
                    uint x_coord, y_coord;
                    uint pixel;
                    int flipped_flag = 0;
                    int pixel_flipped = 0;

                    // Loop through rows 0 to N of sprite
                    for (uint n = 0; n < height; n++)
                    {
                        y_coord = Y + n;

                        // Check if Y coordinate is on the screen
                        if (y_coord < 32)
                        {
                            // Retrieve the new row from memory
                            row_data = mem[I + n];

                            // Loop through each pixel in row (0 to 8)
                            for (int i = 0; i < 8; i++)
                            {
                                x_coord = (uint)(X + i);

                                if (x_coord < 64)
                                {
                                    pixel = (uint)((row_data & 0x80 >> i)>>(7-i)); // Retrieve the pixel
                                    pixel_flipped = ((display.gArray[x_coord, y_coord] == 1) && (pixel == 1))? 1 : 0;
                                    flipped_flag = flipped_flag | pixel_flipped; // set flipped flag
                                    display.gArray[x_coord, y_coord] = (byte)(display.gArray[x_coord, y_coord] ^ pixel); // Update pixel array
                                    if (pixel == 1) { display.DrawPixel((int)(x_coord), (int)(y_coord), 1);}
                                    if (pixel_flipped == 1) { display.DrawPixel((int)(x_coord), (int)(y_coord), 0); }
                                }
                            }
                        }
                    }
                    V[0xF] = (uint)(flipped_flag);
                    //display.Draw();
                    PC += 2;
                    break;
                case 0xE000:
                    uint key_vx = V[(opcode & 0x0F00) >> 8];
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E: // EX9E Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block)
                            PC = (key[key_vx] == 1) ? PC + 4 : PC + 2;
                            break;
                        case 0x00A1: // EXA1 Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                            PC = (key[key_vx] == 0) ? PC + 4 : PC + 2;
                            break;
                    }
                    break;
                case 0xF000:
                    // Retrieve value of VX
                    uint regX = (opcode & 0x0F00) >> 8;
                    // Decode low byte
                    switch (opcode & 0x0FF)
                    {
                        case 0x007: // FX07 Sets VX to the value of the delay timer.                                      
                            V[regX] = timd;
                            PC += 2;
                            break;
                        case 0x00A: // FX0A A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event)   
                            foreach (uint k in key)
                            {
                                if (key[k] == 1)
                                {
                                    V[regX] = k;
                                    PC += 2;
                                    break;
                                }                               
                            }
                            break;
                        case 0x015: // FX15 Sets the delay timer to VX.                                      
                            timd = V[regX];
                            PC += 2;
                            break;
                        case 0x018: // FX18 Sets the sound timer to VX.                                      
                            tims = V[regX];
                            PC += 2;
                            break;
                        case 0x01E: // FX1E Add Vx to I                                      
                            I += V[regX];
                            PC += 2;
                            break;
                        case 0x029: // FX29 Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.                                      
                            I = 0x050 + V[regX] * 5;
                            PC += 2;
                            break;
                        case 0x0033:
                            // FX33 Stores the binary-coded decimal representation of VX, 
                            // with the most significant of three digits at the address in I, 
                            // the middle digit at I plus 1, and the least significant digit at I plus 2. 
                            // (In other words, take the decimal representation of VX, place the hundreds digit 
                            // in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.)
                            uint value = V[regX] ;
                            uint hundreds = value / 100;
                            uint tens = (value - hundreds * 100) / 10;
                            uint ones = value - hundreds * 100 - tens * 10;
                            mem[I] = hundreds;
                            mem[I + 1] = tens;
                            mem[I + 2] = ones;
                            PC += 2;
                            break;
                        case 0x055: // FX55 Stores V0 to VX (including VX) in memory starting at address I.
                                    // The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            for (uint x = 0; x <= regX; x++)
                            {
                                mem[I + x] = V[x];
                            }
                            PC += 2;
                            break;
                        case 0x065: // FX65 Fills V0 to VX (including VX) with values from memory starting at address I. 
                                    // The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                            for (uint x = 0; x <= regX; x++)
                            {
                                V[x] = mem[I + x];
                            }
                            PC += 2;
                            break;
                    }
                    break;

            }

            // Execute the opcode

            // Update timers
            timd = (timd > 0) ? --timd : 0;
            tims = (tims > 0) ? --tims : 0;
        }

        public void InitRegs()
        {
            opcode = 0;
            Array.Clear(V, 0, V.Length);
            stack.Clear();
            timd = 0;
            tims = 0;
            PC = 0x200;

            uint[] chip8_fontset =
            {
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
              0x20, 0x60, 0x20, 0x20, 0x70, // 1
              0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
              0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
              0x90, 0x90, 0xF0, 0x10, 0x10, // 4
              0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
              0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
              0xF0, 0x10, 0x20, 0x40, 0x40, // 7
              0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
              0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
              0xF0, 0x90, 0xF0, 0x90, 0x90, // A
              0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
              0xF0, 0x80, 0x80, 0x80, 0xF0, // C
              0xE0, 0x90, 0x90, 0x90, 0xE0, // D
              0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
              0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            for (int x=0; x < chip8_fontset.Length; x++)
            {
                mem[0x050 + x] = chip8_fontset[x];
            }
        }
    }
}
