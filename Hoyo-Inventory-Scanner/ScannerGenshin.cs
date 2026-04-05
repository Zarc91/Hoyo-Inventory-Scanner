using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoyo_Inventory_Scanner
{
    class ScannerGenshin : Scanner {

        public ScannerGenshin(int scan_speed, bool scan_four_stars, bool is_geforce_now, int most_left_position, int most_top_position, int offset_row, int offset_line, int rows_per_line, int lines_before_scrolling, Size screenshot_size, int screenshot_start_x, int screenshot_start_y) : base(scan_speed, scan_four_stars, is_geforce_now, most_left_position, most_top_position, offset_row, offset_line, rows_per_line, lines_before_scrolling, screenshot_size, screenshot_start_x, screenshot_start_y) { }

        public override Artefact analyzeArtefact(Bitmap screenshot)
        {
            throw new NotImplementedException();
        }

        public override bool focusWindow() {
            if (is_geforce_now)
                return WindowHelper.FocusWindow("GeForceNOW");
            else 
                return WindowHelper.FocusWindow("GenshinImpact");
        }

        public override string generateJson()
        {
            throw new NotImplementedException();
        }

        public override bool isValideArtefact(Artefact current_artefact)
        {
            throw new NotImplementedException();
        }
    }
}
