using System.Drawing;
using System.Windows.Forms;

namespace C8
{
    class C8Graphics
    {
        const int G_WIDTH = 64;
        const int G_HEIGHT = 32;

        public PictureBox PB;
        public Graphics g;
        public SolidBrush myBrushBlack = new SolidBrush(Color.Black);
        public SolidBrush myBrushWhite = new SolidBrush(Color.White);
        public byte[,] gArray; //Graphics Array
        public int screen_size = 10;
        public Rectangle[,] rectArray; // An array of rectangles


        public C8Graphics(PictureBox PB1)
        {
            // Set the PictureBox and create gArray
            PB = PB1;
            gArray = new byte[G_WIDTH,G_HEIGHT];
            rectArray = new Rectangle[G_WIDTH, G_HEIGHT];

            g = PB.CreateGraphics();

            // Fill array of rectangles with black rectangles
            for (int y = 0; y < G_HEIGHT; y++)
            {
                for (int x = 0; x < G_WIDTH; x++)
                {
                    Rectangle myRect = new Rectangle(x * screen_size, y * screen_size, screen_size, screen_size);
                    rectArray[x, y] = myRect;
                }
            }

            // Initialize/Clear Display
            Clear();
        }

        // Set the PictureBox and graphics array to default values
        public void Clear()
        {
            for (int y = 0; y < G_HEIGHT; y++)
            {
                for (int x = 0; x < G_WIDTH; x++)
                {
                    gArray[x,y] = 0x00;
                    g.FillRectangle(myBrushBlack, rectArray[x,y]);
                }
            }
        }

        // Draw a pixel at x, y with colour val (0 = black, >0 = white)
        public void DrawPixel(int x, int y, int val)
        {
            if (val == 0) {
                g.FillRectangle(myBrushBlack, rectArray[x, y]);
            } else
            {
                g.FillRectangle(myBrushWhite, rectArray[x, y]);
            }

        }
    }
}
