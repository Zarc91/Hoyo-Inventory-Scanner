using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Threading;

namespace Hoyo_Inventory_Scanner
{
    abstract class Scanner {
        public int scan_speed;
        public bool scan_four_stars;
        public bool is_geforce_now;
        public int most_left_position;
        public int most_top_position;
        public int offset_row;
        public int offset_line;
        public int rows_per_line;
        public int lines_before_scrolling;
        public Size screenshot_size;
        public int screenshot_start_x;
        public int screenshot_start_y;
        public List<Artefact> artefacts;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_MOVE = 0x0001;

        public Scanner(int scan_speed, bool scan_four_stars, bool is_geforce_now, int most_left_position, int most_top_position, int offset_row, int offset_line, int rows_per_line, int lines_before_scrolling, Size screenshot_size, int screenshot_start_x, int screenshot_start_y) {
            this.scan_speed = scan_speed;
            this.scan_four_stars = scan_four_stars;
            this.is_geforce_now = is_geforce_now;
            this.most_left_position = most_left_position;
            this.most_top_position = most_top_position;
            this.offset_line = offset_line;
            this.offset_row = offset_row;
            this.rows_per_line = rows_per_line;
            this.lines_before_scrolling = lines_before_scrolling;
            this.screenshot_size = screenshot_size;
            this.screenshot_start_x = screenshot_start_x;
            this.screenshot_start_y = screenshot_start_y;
            artefacts = new List<Artefact>();
        }

        public void scan() {
            bool success;
            int current_row = 0;
            int current_line = 0;
            Artefact current_artefact;
            Bitmap screenshot;

            success = focusWindow();
            if(!success) {
                Logger.LogDebug("Error : the game window was not found.");
                return;
            }

            SetCursorPos(most_left_position, most_top_position);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            screenshot = takeScreenshot();

            current_artefact = analyzeArtefact(screenshot);

            while(isValideArtefact(current_artefact)) {
                artefacts.Add(current_artefact);
                moveCursorToNextArtefact(ref current_row, ref current_line);
                screenshot = takeScreenshot();
                current_artefact = analyzeArtefact(screenshot);

                if(Keyboard.IsKeyDown(Key.Escape))
                    return;

                Thread.Sleep((int)scan_speed);
            }

        }

        public abstract string generateJson();

        public void moveCursorToNextArtefact(ref int current_row, ref int current_line) {
            // cursor not at the last row
            if(current_row < rows_per_line - 1) {
                current_row += 1;
                SetCursorPos(most_left_position + current_row * offset_row, most_top_position + current_line * offset_line);
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            // cursor at the last row
            else {
                // not last line before having to scroll 
                if(current_line < lines_before_scrolling - 1) {
                    current_line += 1;
                    current_row = 0;
                    SetCursorPos(most_left_position + current_row * offset_row, most_top_position + current_line * offset_line);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }
                // last line and row, have to scroll up smoothly
                else {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    Thread.Sleep(1000);
                    for (int i = 0; i < most_top_position + offset_line * lines_before_scrolling; i++)
                    {
                        mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, 0);
                        Thread.Sleep(1);
                    }
                    for (int i = 0; i < 1073 - (most_top_position + offset_line * lines_before_scrolling) + offset_line * (lines_before_scrolling + 1); i++) {
                        mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, 0);
                        Thread.Sleep(3);
                    }
                    Thread.Sleep(1000);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    Thread.Sleep(1000);
                    current_line = 0;
                    current_row = 0;
                    SetCursorPos(most_left_position + current_row * offset_row, most_top_position + current_line * offset_line);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }
            }
        }

        public Bitmap takeScreenshot() {
            Bitmap captureBitmap = new Bitmap(screenshot_size.Width, screenshot_size.Height);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(screenshot_start_x, screenshot_start_y, 0, 0, screenshot_size);
            return captureBitmap;
        }

        public abstract bool focusWindow();

        public abstract Artefact analyzeArtefact(Bitmap screenshot);

        public abstract bool isValideArtefact(Artefact current_artefact);

        public void test() {
            bool success;
            int current_row = 0;
            int current_line = 0;
            Artefact current_artefact;
            Bitmap screenshot;

            success = focusWindow();
            if (!success) {
                Logger.LogDebug("Error : the game window was not found.");
                return;
            }

            Thread.Sleep(500);
            Logger.LogDebug("moved : " + SetCursorPos(most_left_position, most_top_position));
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            for (int i = 0; i < 8 * 8; i++) {
                Thread.Sleep((int)scan_speed);
                moveCursorToNextArtefact(ref current_row, ref current_line);
                if (Keyboard.IsKeyDown(Key.Escape))
                    return;
            }

        }

    }
}
