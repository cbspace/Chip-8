use gtk::prelude::*;
use gtk::{Application, ApplicationWindow, Menu, MenuBar, MenuItem, Orientation};
use glib::clone;
use std::io;
use std::ops::Sub;

fn main() {
    let app = Application::builder()
        .application_id("cbspace.chip8")
        .build();
    
    app.connect_activate(|app| {
        build_ui(&app);
    });
    
    app.run();
}

fn build_ui(app: &gtk::Application) {
    
    let gtk_box = gtk::Box::builder()
        .orientation(Orientation::Vertical)
        .build();

    let window = ApplicationWindow::builder()
        .application(app)
        .default_width(320)
        .default_height(240)
        .title("Chip-8")
        .child(&gtk_box)
        .build();
    
    let menu_bar = MenuBar::new();
    
    let emulator_menu = MenuItem::with_label("Emulator");
    let about_menu = MenuItem::with_label("About");

    let emulator_sub_menu = Menu::new();
    let open_menu_item = MenuItem::with_label("Open ROM");
    let quit_menu_item = MenuItem::with_label("Quit");
    emulator_sub_menu.append(&open_menu_item);
    emulator_sub_menu.append(&quit_menu_item);

    open_menu_item.connect_activate(move |_| {
        run_emulator();
    });

    quit_menu_item.connect_activate(clone!(@weak window => move |_| {
        window.close();
    }));

    emulator_menu.set_submenu(Some(&emulator_sub_menu));
    menu_bar.append(&emulator_menu);
    menu_bar.append(&about_menu);

    gtk_box.pack_start(&menu_bar, false, false, 0);
    window.show_all();
}

fn run_emulator() {
    let mut pc: u16 = 0x200;
    let mut vreg: Vec<u8> = vec![0; 16];
    let mut ireg: u16 = 0;

    let mut memory: Vec<u8> = vec![0; 0x10000];
    let mut stack: Vec<u16> = vec![];

    load_font_set(&mut memory);

    let file_path = "../roms/test.c8";
    match load_file(file_path, &mut memory) {
        Ok(()) => println!("Loaded ROM: {}", file_path),
        Err(error) => println!("Error: {}", error)
    };

    for i in 0..10 {
        println!("{:#06x}", pc);
        cpu_cycle(&mut pc, &mut vreg, &mut ireg, &mut memory, &mut stack);
    }

    //test_instruction(&mut pc, &mut vreg, &mut ireg, &mut memory, &mut stack);
}

fn test_instruction(pc: &mut u16, vreg: &mut Vec<u8>,ireg: &mut u16, memory: &mut Vec<u8>, stack: &mut Vec<u16>) {
    memory[0x200] = 0x80;
    memory[0x201] = 0x15;
    vreg[0] = 10;
    vreg[1] = 4;
    cpu_cycle(pc, vreg, ireg, memory, stack);
    println!("V0: {}, V1: {}, VF: {}", vreg[0], vreg[1], vreg[0xf]);
    println!("Expect {}", 10-4);
    
    *pc = 0x200;
    memory[0x200] = 0x80;
    memory[0x201] = 0x15;
    vreg[0] = 4;
    vreg[1] = 10;
    cpu_cycle(pc, vreg, ireg, memory, stack);
    println!("V0: {}, V1: {}, VF: {}", vreg[0], vreg[1], vreg[0xf]);
    println!("Expect {}", (4+256)-10);
}

fn load_file(file_path: &str, memory: &mut Vec<u8>) -> Result<(), io::Error> {
    let bytes = std::fs::read(file_path)?;
    let mut address = 0x200;
    for byte in bytes {
        memory[address] = byte;
        address += 1;
    }
    Ok(())
}

fn load_font_set(memory: &mut Vec<u8>) {
    let chip8_fontset: Vec<u8> = vec![
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
    ];

    for (index, byte) in chip8_fontset.iter().enumerate() {
        memory[0x50 + index] = *byte;
    }
}

fn cpu_cycle(pc: &mut u16, vreg: &mut Vec<u8>,ireg: &mut u16, memory: &mut Vec<u8>, stack: &mut Vec<u16>) {
    let ins: u16 = ((memory[*pc as usize] as u16) << 8) + memory[(*pc + 1) as usize] as u16;
    let n1: usize = (memory[*pc as usize] as usize) >> 4;
    let n2: usize = (memory[*pc as usize] as usize) & 0x0f;
    let n3: usize = (memory[(*pc + 1) as usize] as usize) >> 4;
    let n4: usize = (memory[(*pc + 1) as usize] as usize) & 0x0f;
    println!("{:#06x}", ins);
    println!("");

    match ins & 0xf000 {
        0x0000 => { 
            match ins & 0x0fff {
                0x000 => *pc += 2,         // 0NNN Call RCA 1802 program at address NNN
                0x0E0 => *pc += 2,         // 00E0 Clear the screen
                0x0EE => {                 // 00EE Return from subroutine
                    let stack_value = stack.pop();
                    match stack_value {
                        Some(v) => *pc = v + 2,
                        None => { println!("Error: Stack Underflow");
                                  *pc += 2;
                        }
                    } 
                }, 
                _ => { println!("Error: Invalid Instruction");
                       *pc += 2;
                }
            }
        },
        0x1000 => *pc = ins & 0x0fff,        // 1NNN - Jump to NNN
        0x2000 => { stack.push(*pc);         // 2NNN Call subroutine at address NNN
                    *pc = ins & 0x0fff;
                  },        
        0x3000 => { *pc += if memory[n2] == (ins & 0xff) as u8 
                    { 4 } else { 2 };        // 3XNN Skip next instruction if VX = NN   
                  },                         
        0x4000 => { *pc += if memory[n2] != (ins & 0xff) as u8 
                    { 4 } else { 2 };        // 4XNN Skip next instruction if VX != NN   
                  },
        0x5000 => { *pc += if memory[n2] == memory[n3]
                    { 4 } else { 2 };        // 5XY0 Skip next instruction if VX = VY                    
                  },
        0x6000 => {                          // 6XNN - Set VX to NN
                    vreg[n2] = (ins & 0x00ff) as u8; 
                    *pc += 2; 
                  },
        0x7000 => {                         // 7XNN Add NN to VX (Carry flag not changed)  
                    vreg[n2] = vreg[n2] + (ins & 0xff) as u8;
                    *pc += 2; 
                  },
        0x8000 => { match n4 {
                        0x0 => {            // 8XY0 Sets the value of VX to VY      
                            vreg[n2] = vreg[n3];
                            *pc += 2;
                        },
                        0x1 => {            // 8XY1 Sets VX to VX or VY (Bitwise OR operation) 
                            vreg[n2] |= vreg[n3];
                            *pc += 2;
                        },
                        0x2 => {            // 8XY2 Sets VX to VX & VY (Bitwise AND operation) 
                            vreg[n2] &= vreg[n3];
                            *pc += 2;
                        },
                        0x3 => {            // 8XY3 Sets VX to VX XOR VY (Bitwise OXR operation) 
                            vreg[n2] ^= vreg[n3];
                            *pc += 2;
                        },
                        0x4 => {            // 8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't. 
                            let sum: u16 = vreg[n2] as u16 + vreg[n3] as u16;
                            vreg[0xf] = if sum > 255 { 1 } else { 0 };
                            vreg[n2] = (sum % 0x100) as u8;
                            *pc += 2;
                        },
                        0x5 => {            // 8XY5 VX = VX - VY. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            let difference: i16 = vreg[n2] as i16 - vreg[n3] as i16;
                            if difference >= 0 {
                                vreg[n2] = difference as u8;
                                vreg[0xf] = 1;
                            } else {
                                vreg[n2] = (difference + 256) as u8;
                                vreg[0xf] = 0;
                            }
                            *pc += 2;
                        },
                        0x6 => {            // 8XY6 Stores the least significant bit of VX in VF and then shifts VX to the right by 1 
                            vreg[0xf] = vreg[n2] & 0x01;
                            vreg[n2] >>= 1; 
                            *pc += 2;
                        },
                        0x7 => {            // 8XY7 VX = VY - VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            let difference: i16 = vreg[n3] as i16 - vreg[n2] as i16;
                            if difference >= 0 {
                                vreg[n2] = difference as u8;
                                vreg[0xf] = 1;
                            } else {
                                vreg[n2] = (difference + 256) as u8;
                                vreg[0xf] = 0;
                            }
                            *pc += 2;
                        },
                        _ => println!("Invalid Instruction: {:#06x}", ins)
                  }
        },
        0x9000 => {  },        // 9NNN -
        0xA000 => {  },        // ANNN -
        0xB000 => {  },        // BNNN -
        0xC000 => {  },        // CNNN -
        0xD000 => {  },        // DNNN -
        0xE000 => {  },        // ENNN -
        0xF000 => {  },        // FNNN -
        _ => {}
    }
}