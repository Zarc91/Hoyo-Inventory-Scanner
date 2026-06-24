using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace Hoyo_Inventory_Scanner
{
    class ScannerGenshin : Scanner {

        public ScannerGenshin(int scan_speed, bool is_geforce_now, int most_left_position, int most_top_position, int offset_row, int offset_line, int rows_per_line, int lines_before_scrolling, Size screenshot_size, int screenshot_start_x, int screenshot_start_y) : base(scan_speed, is_geforce_now, most_left_position, most_top_position, offset_row, offset_line, rows_per_line, lines_before_scrolling, screenshot_size, screenshot_start_x, screenshot_start_y) { }

        public override Artefact analyzeArtefact(Bitmap screenshot) {
            string slot = string.Empty;
            string location = string.Empty;
            string mainstat = string.Empty;
            string level = string.Empty;
            string set = string.Empty;
            Substat[] substats = new Substat[4];
            string ocrtext = string.Empty;
            int offset = 0;
            Stopwatch stopwatch = new Stopwatch();

            Bitmap grey_screenshot = new Grayscale(0.2125, 0.7154, 0.0721).Apply(screenshot);
            Bitmap gamma_screenshot = new GammaCorrection(0.3).Apply(grey_screenshot);
            new ContrastCorrection(100).ApplyInPlace(grey_screenshot);
            new ContrastCorrection(100).ApplyInPlace(gamma_screenshot);
            Bitmap inverted_grey_screenshot = new Invert().Apply(grey_screenshot);

            TesseractEngine engine = new TesseractEngine(@"./tessdata", "genshin_fast", EngineMode.LstmOnly);
            stopwatch.Start();

            // slot : flower, plume, sands, goblet or circlet
            Pix screenshot_slot = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(25, 13, 80, 26), inverted_grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_slot)) {
                ocrtext = page.GetText();
                slot = ocrtext.Trim().Split(' ')[0].ToLower();
            }
            
            // which character equipped this artefact
            Pix screenshot_location = PixConverter.ToPix(grey_screenshot.Clone(new Rectangle(80, 740, 400, 30), grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_location)) {
                ocrtext = page.GetText();
                if (ocrtext.Contains("Equipped:"))
                    location = ocrtext.Trim().Replace("Equipped:", string.Empty).Replace(" ", string.Empty);
            }

            // mainstat : 
            // flower : hp
            // plume : atk                                                                                      
            // sands : atk_, hp_, def_, eleMas, enerRech_
            // goblet : atk_, hp_, def_, eleMas, physical_dmg_, anemo_dmg_, geo_dmg_, electro_dmg_, hydro_dmg_, pyro_dmg_, cryo_dmg_, dendro_dmg_
            // circlet : atk_, hp_, def_, eleMas, critRate_, critDMG_, heal_
            switch (slot) {
                case "flower":
                    mainstat = "hp";
                    break;
                case "plume":
                    mainstat = "atk";
                    break;
                default:
                    Pix screenshot_mainstat = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(20, 93, 230, 30), inverted_grey_screenshot.PixelFormat));
                    using (Page page = engine.Process(screenshot_mainstat)) {
                        ocrtext = page.GetText();
                        if (ocrtext.Contains("ATK") || ocrtext.Contains("HP") || ocrtext.Contains("DEF"))
                            mainstat = ocrtext.Trim().ToLower() + "_";
                        if (ocrtext.Contains("DMG Bonus"))
                            mainstat = ocrtext.Trim().Split(' ')[0].ToLower() + "_dmg_";
                        if (ocrtext.Contains("Elemental Mastery"))
                            mainstat = "eleMas";
                        if (ocrtext.Contains("Energy Recharge"))
                            mainstat = "enerRech_";
                        if (ocrtext.Contains("Healing Bonus"))
                            mainstat = "heal_";
                        if (ocrtext.Contains("CRIT"))
                            mainstat = "crit" + ocrtext.Trim().Split(' ')[1] + "_";
                    }
                    break;
            }

            // sanctifying elixir ?
            Pix screenshot_sanctifying = PixConverter.ToPix(grey_screenshot.Clone(new Rectangle(56, 236, 126, 26), grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_sanctifying)) {
                ocrtext = page.GetText();
            }
            if (ocrtext.Contains("Sanctifying"))
                offset = 38;

            // level
            Pix screenshot_level = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(32, 253 + offset, 50, 22), inverted_grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_level)) {
                ocrtext = page.GetText();
                level = ocrtext.Trim().Replace("+", string.Empty);
            }

            // which set
            Pix screenshot_set = PixConverter.ToPix(gamma_screenshot.Clone(new Rectangle(26, 457 + offset, 459, 54), gamma_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_set)) {
                ocrtext = page.GetText();
                set = ocrtext.Replace("\n", " ");
                set = set.Trim().Split(':')[0];
                set = string.Join(" ", set.Split(' ', '-').Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
                set = set.Replace(" ", string.Empty).Replace("'", string.Empty).Replace("-", string.Empty);
            }

            // substats : atk, hp, def, atk_, hp_, def_, eleMas, enerRech_, critRate_, critDMG_
            Pix screenshot_sub = PixConverter.ToPix(screenshot.Clone(new Rectangle(47, 300 + offset, 303, 150), screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_sub)) {
                ocrtext = page.GetText().Trim();
                int i = 0;
                foreach (string substat in ocrtext.Split('\n')) {
                    if (!substat.Equals(string.Empty)) {
                        if (substat.Contains("CRIT"))
                            substats[i] = new Substat("crit" + substat.Split('+')[0].Split(' ')[1], substat.Split('+')[1]);
                        if (substat.Contains("Elemental Mastery"))
                            substats[i] = new Substat("eleMas", substat.Split('+')[1]);
                        if (substat.Contains("Energy Recharge"))
                            substats[i] = new Substat("enerRech", substat.Split('+')[1].Replace("¥", string.Empty));
                        if (substat.Contains("ATK") || substat.Contains("HP") || substat.Contains("DEF"))
                            substats[i] = new Substat(substat.Split('+')[0].ToLower(), substat.Split('+')[1]);
                        // if value is a %, add _ to the stat key
                        if (substats[i].value.Contains("%")) {
                            substats[i].stat += "_";
                            substats[i].value = substats[i].value.Replace("%", string.Empty);
                        }
                        i++;
                    }
                }
            }

            stopwatch.Stop();

            Logger.LogDebug("-----------------");
            Logger.LogDebug("set : " + set);
            Logger.LogDebug("slot : " + slot);
            Logger.LogDebug("location : " + location);
            Logger.LogDebug("mainstat : " + mainstat);
            Logger.LogDebug("level : " + level);
            foreach (Substat substat in substats)
            {
                Logger.LogDebug("substat : " + substat.stat + " + " + substat.value);
            }
            Logger.LogDebug("execution time : " + stopwatch.ElapsedMilliseconds + "ms");

            return new Artefact(set, slot, location, mainstat, level, substats);
        }

        public override bool focusWindow() {
            if (is_geforce_now)
                return WindowHelper.FocusWindow("GeForceNOW");
            else 
                return WindowHelper.FocusWindow("GenshinImpact");
        }

        public override string generateExport() {
            string art = "";
            foreach (Artefact artefact in artefacts) {
                art += artefact.generateExport() + ",";
            }
            art = art.Remove(art.Length - 1);
            return "{\"format\":\"GOOD\",\"source\":\"Hoyo Inventory Scanner\",\"version\":3,\"artifacts\":[" + art + "]}";
        }

        public override bool isValideArtefact(Artefact current_artefact) {
            if (current_artefact.level.Equals("20"))
                return true;
            else
                return false;
        }

        public override async Task<Artefact> analyzeArtefactAsync(Bitmap screenshot) {
            string slot = string.Empty;
            string location = string.Empty;
            string mainstat = string.Empty;
            string level = string.Empty;
            string set = string.Empty;
            Substat[] substats = new Substat[4];
            string ocrtext = string.Empty;
            int offset = 0;
            Stopwatch stopwatch = new Stopwatch();

            Bitmap grey_screenshot = new Grayscale(0.2125, 0.7154, 0.0721).Apply(screenshot);
            Bitmap gamma_screenshot = new GammaCorrection(0.3).Apply(grey_screenshot);
            new ContrastCorrection(100).ApplyInPlace(grey_screenshot);
            new ContrastCorrection(100).ApplyInPlace(gamma_screenshot);
            Bitmap inverted_grey_screenshot = new Invert().Apply(grey_screenshot);

            TesseractEngine engine = new TesseractEngine(@"./tessdata", "genshin_fast", EngineMode.LstmOnly);
            TesseractEngine engine1 = new TesseractEngine(@"./tessdata", "genshin_fast", EngineMode.LstmOnly);
            TesseractEngine engine2 = new TesseractEngine(@"./tessdata", "genshin_fast", EngineMode.LstmOnly);
            stopwatch.Start();

            // slot : flower, plume, sands, goblet or circlet
            Pix screenshot_slot = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(25, 13, 80, 26), inverted_grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_slot)) {
                ocrtext = page.GetText();
                slot = ocrtext.Trim().Split(' ')[0].ToLower();
            }
            
            // which character equipped this artefact
            Pix screenshot_location = PixConverter.ToPix(grey_screenshot.Clone(new Rectangle(80, 740, 400, 30), grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_location)) {
                ocrtext = page.GetText();
                if (ocrtext.Contains("Equipped:"))
                    location = ocrtext.Trim().Replace("Equipped:", string.Empty).Replace(" ", string.Empty);
            }

            // mainstat : 
            // flower : hp
            // plume : atk                                                                                      
            // sands : atk_, hp_, def_, eleMas, enerRech_
            // goblet : atk_, hp_, def_, eleMas, physical_dmg_, anemo_dmg_, geo_dmg_, electro_dmg_, hydro_dmg_, pyro_dmg_, cryo_dmg_, dendro_dmg_
            // circlet : atk_, hp_, def_, eleMas, critRate_, critDMG_, heal_
            switch (slot) {
                case "flower":
                    mainstat = "hp";
                    break;
                case "plume":
                    mainstat = "atk";
                    break;
                default:
                    Pix screenshot_mainstat = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(20, 93, 230, 30), inverted_grey_screenshot.PixelFormat));
                    using (Page page = engine.Process(screenshot_mainstat)) {
                        ocrtext = page.GetText();
                        if (ocrtext.Contains("ATK") || ocrtext.Contains("HP") || ocrtext.Contains("DEF"))
                            mainstat = ocrtext.Trim().ToLower() + "_";
                        if (ocrtext.Contains("DMG Bonus"))
                            mainstat = ocrtext.Trim().Split(' ')[0].ToLower() + "_dmg_";
                        if (ocrtext.Contains("Elemental Mastery"))
                            mainstat = "eleMas";
                        if (ocrtext.Contains("Energy Recharge"))
                            mainstat = "enerRech_";
                        if (ocrtext.Contains("Healing Bonus"))
                            mainstat = "heal_";
                        if (ocrtext.Contains("CRIT"))
                            mainstat = "crit" + ocrtext.Trim().Split(' ')[1] + "_";
                    }
                    break;
            }

            // sanctifying elixir ?
            Pix screenshot_sanctifying = PixConverter.ToPix(grey_screenshot.Clone(new Rectangle(56, 236, 126, 26), grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_sanctifying)) {
                ocrtext = page.GetText();
            }
            if (ocrtext.Contains("Sanctifying"))
                offset = 38;

            // level
            Pix screenshot_level = PixConverter.ToPix(inverted_grey_screenshot.Clone(new Rectangle(32, 253 + offset, 50, 22), inverted_grey_screenshot.PixelFormat));
            using (Page page = engine.Process(screenshot_level)) {
                ocrtext = page.GetText();
                level = ocrtext.Trim().Replace("+", string.Empty);
            }

            var task_set = Task.Run(() => {
                // which set
                Pix screenshot_set = PixConverter.ToPix(gamma_screenshot.Clone(new Rectangle(26, 457 + offset, 459, 54), gamma_screenshot.PixelFormat));
                using (Page page = engine1.Process(screenshot_set)) {
                    ocrtext = page.GetText();
                    set = ocrtext.Replace("\n", " ");
                    set = set.Trim().Split(':')[0];
                    set = string.Join(" ", set.Split(' ', '-').Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
                    set = set.Replace(" ", string.Empty).Replace("'", string.Empty).Replace("-", string.Empty);
                }
            });

            var task_substats = Task.Run(() => {
                // substats : atk, hp, def, atk_, hp_, def_, eleMas, enerRech_, critRate_, critDMG_
                Pix screenshot_sub = PixConverter.ToPix(screenshot.Clone(new Rectangle(47, 300 + offset, 303, 150), screenshot.PixelFormat));
                using (Page page = engine2.Process(screenshot_sub)) {
                    ocrtext = page.GetText().Trim();
                    int i = 0;
                    foreach (string substat in ocrtext.Split('\n')) {
                        if (!substat.Equals(string.Empty)) {
                            if (substat.Contains("CRIT"))
                                substats[i] = new Substat("crit" + substat.Split('+')[0].Split(' ')[1], substat.Split('+')[1]);
                            if (substat.Contains("Elemental Mastery"))
                                substats[i] = new Substat("eleMas", substat.Split('+')[1]);
                            if (substat.Contains("Energy Recharge"))
                                substats[i] = new Substat("enerRech", substat.Split('+')[1]);
                            if (substat.Contains("ATK") || substat.Contains("HP") || substat.Contains("DEF"))
                                substats[i] = new Substat(substat.Split('+')[0].ToLower(), substat.Split('+')[1]);
                            // if value is a %, add _ to the stat key
                            if (substats[i].value.Contains("%")) {
                                substats[i].stat += "_";
                                substats[i].value = substats[i].value.Replace("%", string.Empty);
                            }
                            i++;
                        }
                    }
                }
            });

            await Task.WhenAll(task_set, task_substats);
            stopwatch.Stop();

            Logger.LogDebug("-----------------");
            Logger.LogDebug("set : " + set);
            Logger.LogDebug("slot : " + slot);
            Logger.LogDebug("location : " + location);
            Logger.LogDebug("mainstat : " + mainstat);
            Logger.LogDebug("level : " + level);
            foreach (Substat substat in substats)
            {
                Logger.LogDebug("substat : " + substat.stat + " + " + substat.value);
            }
            Logger.LogDebug("execution time : " + stopwatch.ElapsedMilliseconds + "ms");

            return new Artefact(set, slot, location, mainstat, level, substats);
        }
    }
}
