using AForge.Imaging.Filters;
using System.Drawing;
using System.Linq;
using Tesseract;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;

namespace Hoyo_Inventory_Scanner
{
    class ScannerHSR : Scanner
    {
        public Dictionary<string, string> set_id = new Dictionary<string, string>();

        public ScannerHSR(int scan_speed, bool is_geforce_now, int most_left_position, int most_top_position, int offset_row, int offset_line, int rows_per_line, int lines_before_scrolling, Size screenshot_size, int screenshot_start_x, int screenshot_start_y) : base(scan_speed, is_geforce_now, most_left_position, most_top_position, offset_row, offset_line, rows_per_line, lines_before_scrolling, screenshot_size, screenshot_start_x, screenshot_start_y)
        {
            using (HttpClient client = new HttpClient())
            {
                Stream text_map = client.GetStreamAsync("https://gitlab.com/Dimbreath/turnbasedgamedata/-/raw/main/TextMap/TextMapEN.json").Result;
                Stream relic_set_config = client.GetStreamAsync("https://gitlab.com/Dimbreath/turnbasedgamedata/-/raw/main/ExcelOutput/RelicSetConfig.json").Result;
                StreamReader reader = new StreamReader(relic_set_config);
                string line = string.Empty;
                string id = string.Empty;
                Dictionary<string, string> tmp = new Dictionary<string, string>();

                while ((line = reader.ReadLine()) != null) {
                    if (line.Contains("SetID")) {
                        id = line.Split(':')[1].Trim();
                        while (!(line = reader.ReadLine()).Contains("Hash")) { }
                        tmp.Add(id, line.Split(':')[1].Trim());
                    }
                }

                reader.Close();
                reader = new StreamReader(text_map);
                List<string> keys = tmp.Keys.ToList();

                while (keys.Count > 0) {
                    line = reader.ReadLine();
                    foreach (string key in keys) {
                        if (line.Contains(tmp[key])) {
                            tmp[key] = line.Substring(line.Split(':')[0].Length + 1, line.Length - line.Split(':')[0].Length - 2).Replace("\"", string.Empty).Trim();
                            keys.Remove(key);
                            break;
                        }
                    }
                }

                foreach (var i in tmp) {
                    set_id.Add(i.Value, i.Key);
                }

                reader.Close();
                reader.Dispose();
                text_map.Dispose();
                relic_set_config.Dispose();
            }
        }

        public override bool focusWindow() {
            if (is_geforce_now)
                return WindowHelper.FocusWindow("GeForceNOW");
            else
                return WindowHelper.FocusWindow("StarRail");
        }

        public override string generateExport() {
            string art = "";
            if (artefacts.Count() > 0) {
                foreach (Artefact artefact in artefacts) {
                    art += artefact.generateExport() + ",";
                }
                art = art.Remove(art.Length - 1);
            }
            return "{\"source\":\"HSR-Scanner\",\"build\":\"v1.5.0\",\"version\":4,\"metadata\":{\"trailblazer\":\"Stelle\"},\"relics\":[" + art + "],\"characters\":[]}";
        }

        public override bool isValideArtefact(Bitmap screenshot) {
            string ocrtext = string.Empty;
            string level = string.Empty;
            Bitmap grey_screenshot = new Grayscale(0.2125, 0.7154, 0.0721).Apply(screenshot);
            Bitmap gamma_screenshot = new GammaCorrection(0.3).Apply(grey_screenshot);
            new ContrastCorrection(80).ApplyInPlace(gamma_screenshot);
            Bitmap inverted_grey_screenshot = new Invert().Apply(gamma_screenshot);

            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "hsr_fast", EngineMode.LstmOnly))
            {
                using (Pix screenshot_level = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(10, 117, 70, 32), inverted_grey_screenshot.PixelFormat)))
                {
                    using (Page page = engine.Process(screenshot_level))
                    {
                        ocrtext = page.GetText();
                        level = ocrtext.Replace("+", string.Empty).Trim();
                    }
                }
            }

            grey_screenshot.Dispose();
            inverted_grey_screenshot.Dispose();

            if (level.Equals("15"))
                return true;
            else
                return false;
        }

        public override void analyzeArtefact(Bitmap screenshot) {
            string slot = string.Empty;
            string mainstat = string.Empty;
            string level = string.Empty;
            string set = string.Empty;
            Substat[] substats = new Substat[4];
            string ocrtext = string.Empty;

            Bitmap grey_screenshot = new Grayscale(0.2125, 0.7154, 0.0721).Apply(screenshot);
            Bitmap gamma_screenshot = new GammaCorrection(0.3).Apply(grey_screenshot);
            new ContrastCorrection(80).ApplyInPlace(grey_screenshot);
            new ContrastCorrection(80).ApplyInPlace(gamma_screenshot);
            Bitmap inverted_grey_screenshot = new Invert().Apply(grey_screenshot);
            Bitmap inverted_gamma_screenshot = new Invert().Apply(gamma_screenshot);

            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "hsr_fast", EngineMode.LstmOnly))
            {
                // slot : Head, Hands, Body, Feet, Planar Sphere or Link Rope
                using (Pix screenshot_slot = PixConverter.ToPix(inverted_gamma_screenshot.Clone(new Rectangle(10, 85, 160, 32), inverted_gamma_screenshot.PixelFormat)))
                {
                    using (Page page = engine.Process(screenshot_slot))
                    {
                        ocrtext = page.GetText();
                        slot = ocrtext.Trim();
                    }
                }

                // mainstat : 
                // Head : HP
                // Hands : ATK                                                                                      
                // Body : ATK, HP, DEF, CRIT Rate, CRIT DMG, Outgoing Healing Boost, Effect Hit Rate
                // Feet : ATK, HP, DEF, SPD
                // Planar Sphere : ATK, HP, DEF, Physical DMG Boost, Fire DMG Boost, Ice DMG Boost, Lightning DMG Boost, Wind DMG Boost, Quantum DMG Boost, Imaginary DMG Boost
                // Link Rope : ATK, HP, DEF, Break Effect, Energy Regeneration Rate
                switch (slot)
                {
                    case "Head":
                        mainstat = "HP";
                        break;
                    case "Hands":
                        mainstat = "ATK";
                        break;
                    default:
                        using (Pix screenshot_mainstat = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(40, 205, 310, 30), inverted_grey_screenshot.PixelFormat)))
                        {
                            using (Page page = engine.Process(screenshot_mainstat))
                            {
                                ocrtext = page.GetText();
                                mainstat = ocrtext.Trim();
                                // problem with the ocr reading "I" 
                                mainstat.Replace("|", string.Empty);
                            }
                        }
                        break;
                }

                // level
                using (Pix screenshot_level = PixConverter.ToPix(inverted_gamma_screenshot.Clone(new Rectangle(10, 117, 70, 32), inverted_gamma_screenshot.PixelFormat)))
                {
                    using (Page page = engine.Process(screenshot_level))
                    {
                        ocrtext = page.GetText();
                        level = ocrtext.Replace("+", string.Empty).Trim();
                    }
                }

                // which set
                using (Pix screenshot_set = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(0, 410, 435, 65), inverted_grey_screenshot.PixelFormat)))
                {
                    using (Page page = engine.Process(screenshot_set))
                    {
                        ocrtext = page.GetText();
                        set = ocrtext.Replace("\n", " ");
                        set = set.Trim().Split(new string[] { "2-Pc" }, System.StringSplitOptions.None)[0].Trim();
                        // problem with the ocr reading "I"
                        if (set.Contains("|")) {
                            int index = -1;
                            while ((index = set.IndexOf("|", index + 1)) != -1) {
                                if ((index > 0 && set.ElementAt(index - 1) == 'I') || set.ElementAt(index + 1) == 'I')
                                    set = set.Remove(index, 1);
                                else
                                    set = set.Substring(0, index) + "I" + set.Substring(index + 1);
                            }
                        }
                        // problem with the ocr reading "Snow"
                        if (set.Contains("Smow"))
                            set = set.Replace("Smow", "Snow");
                        set = set_id[set].Replace(",", string.Empty);
                    }
                }

                // substats : ATK, HP, DEF, ATK_, HP_, DEF_, SPD, CRIT Rate_, CRIT DMG_, Break Effect_, Effect Hit Rate_, Effect RES_
                using (Bitmap sub = new Bitmap(243 + 79, 155))
                {
                    using (Graphics g = Graphics.FromImage(sub))
                    {
                        g.DrawImage(inverted_grey_screenshot.Clone(new Rectangle(360, 245, 79, 155), inverted_grey_screenshot.PixelFormat), 0, 0);
                        g.DrawImage(inverted_grey_screenshot.Clone(new Rectangle(37, 245, 243, 155), inverted_grey_screenshot.PixelFormat), 79, 0);
                    }
                    using (Pix screenshot_sub = PixConverter.ToPix(sub))
                    {
                        using (Page page = engine.Process(screenshot_sub))
                        {
                            ocrtext = page.GetText().Trim();
                        
                            int i = 0;
                            foreach (string substat in ocrtext.Split('\n')) {
                                if (!substat.Equals(string.Empty)) {
                                    string stat = string.Empty;
                                    int length = substat.Split(' ').Length;
                                    for (int x = 1; x < length; x++) {
                                        stat += substat.Split(' ')[x] + " ";
                                    }
                                    substats[i] = new Substat(stat.Trim(), substat.Split(' ')[0]);
                                    // if value is a %, add _ to the stat key
                                    if (substats[i].value.Contains("%")) {
                                        substats[i].stat += "_";
                                        substats[i].value = substats[i].value.Replace("%", string.Empty);
                                    }
                                    // problem with the ocr reading "I"
                                    if (substats[i].stat.Contains("|"))
                                    {
                                        int index = -1;
                                        while ((index = substats[i].stat.IndexOf("|", index + 1)) != -1)
                                        {
                                            if ((index > 0 && substats[i].stat.ElementAt(index - 1) == 'I') || substats[i].stat.ElementAt(index + 1) == 'I')
                                                substats[i].stat = substats[i].stat.Remove(index, 1);
                                            else
                                                substats[i].stat = substats[i].stat.Substring(0, index) + "I" + substats[i].stat.Substring(index + 1);
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                    }
                }
            }

            grey_screenshot.Dispose();
            gamma_screenshot.Dispose();
            inverted_grey_screenshot.Dispose();
            inverted_gamma_screenshot.Dispose();

            artefacts.Add(new ArtefactHSR(set, slot, string.Empty, mainstat, level, substats));
        }
    }
}