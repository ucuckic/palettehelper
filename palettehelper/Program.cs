using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace palettehelper
{
    class Program
    {
        public static string[] autofixsidescharacters = { "hilda","seth","rentaro" };

        public static void Main(string[] args)
        {
            string workingdir = Directory.GetCurrentDirectory();
            string mode = "unist";
            bool editsave = false;

            /*
            string PSPALdir = Path.Combine(workingdir, none);
            string UNIPALdir = Path.Combine(workingdir, none);
            string outputdir = Path.Combine(workingdir, none);
            string txtlistdir = Path.Combine(workingdir, none);
            */

            string PSPALdir = "none";
            string UNIPALdir = "none";
            string outputdir = "none";

            string palnumstring = "0";
            string alphacolorstring = "255";

            List<string> outputdirs = new List<string>();

            byte alphacolor = Convert.ToByte(alphacolorstring);

            int palnum = Convert.ToInt32(palnumstring);

            int output_type = 0;


            List<string> pspaldirtextlist = new List<string>();

            if (args.Length > 0)
            {
                int count = 0;
                foreach (string arg in args)
                {
                    //Console.WriteLine(args.Length+" "+arg);
                    switch (arg)
                    {
                        case "-input":
                            PSPALdir = @args[count + 1];
                            break;
                        case "-basepal":
                            UNIPALdir = @args[count + 1];
                            break;
                        case "-color":
                            palnumstring = args[count + 1];
                            break;
                        case "-output":
                            outputdir = @args[count + 1];

                            outputdirs.Add(outputdir);

                            break;
                        case "-mode":
                            mode = args[count + 1];
                            break;
                        case "-sides":
                            break;
                        case "-editsave":
                            editsave = true;

                            Console.WriteLine("save edit");
                            break;
                        case "-basealpha":
                            alphacolor = Convert.ToByte(args[count + 1]);
                            break;
                        case "-output_type":
                            output_type = Convert.ToByte(args[count + 1]);
                            break;
                    }
                    count++;
                    //Console.WriteLine(count);
                }
                //System.Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Syntax: -input MS.Pal|Pal List.txt|Pal.png|, -basepal UNI.Pal, -color Pal to replace, -output output file");
                return;
            }

            if(editsave)
            {
                if (File.Exists(PSPALdir))
                {
                    Console.WriteLine("writing save");
                    PalMethods.createfile(outputdir, PalMethods.editsave(File.ReadAllBytes(PSPALdir)));
                    return;
                }
                else
                {
                    Console.WriteLine("save file not found");
                    return;
                }
            }

            byte[] basepalbytes = new byte[86032];

            int type = 0;

            string PSPALfn = Path.GetFileName(PSPALdir);

            int headeroff = 0;

            PalFile basepal = new PalFile();
            Palette inpal = new Palette();
            if (File.Exists(UNIPALdir))
            {
                basepalbytes = File.ReadAllBytes(UNIPALdir);
                type = PalMethods.determineinput(basepalbytes);
                switch (type)
                {
                    case 0:
                        headeroff = 16; //st mode
                        break;
                    case 1:
                        headeroff = 4; //el mode
                        break;
                    case 2:
                        break;
                    case 3: //text list
                        Console.WriteLine("textfile");
                        break;
                    case 10:
                        //headeroff = 0;
                        break;
                }
                basepal = PalMethods.loadpals(basepalbytes);
            }
            byte[] PSPAL = new byte[1024];

            if (File.Exists(PSPALdir))
            {
                
                //inpal = PalMethods.loadpalette(PSPAL, 100);
            

                if (PSPALfn.EndsWith(".txt"))
                {
                    pspaldirtextlist = File.ReadAllLines(PSPALdir).ToList();
                    Console.WriteLine("a text file " + pspaldirtextlist.Count);
                    for (int i = 0; i < pspaldirtextlist.Count; i++)
                    {
                        if (pspaldirtextlist[i] != "")
                        {
                            List<string> config_list = new List<string>();

                            Regex match_params = new Regex(@"\[(.*?)\]");

                            MatchCollection line_parameters = match_params.Matches(pspaldirtextlist[i]);

                            foreach(Match match in line_parameters)
                            {
                                config_list.Add(pspaldirtextlist[i].Substring(match.Index+1,match.Length-2));
                            }

                            //Console.WriteLine("early config match count "+line_parameters.Count+" ind "+ line_parameters[0].Index+" len "+ line_parameters[0].Length);

                            string use_str = (line_parameters.Count > 0)? pspaldirtextlist[i].Substring(0, line_parameters[0].Index) : pspaldirtextlist[i];
                            if (File.Exists(use_str) == false)
                            {
                                string mess = "file not found";

                                Console.WriteLine("skipped " + use_str + " num " + i + " " + " because " + mess);

                                if (i > basepal.Lpals.Count)
                                {
                                    Palette newpal = new Palette();

                                    basepal.Lpals.Add(newpal);
                                }

                                if (i > basepal.Rpals.Count)
                                {
                                    Palette newpal = new Palette();

                                    basepal.Rpals.Add(newpal);
                                }
                            }
                            else
                            {
                                Console.WriteLine("cycling lines " + i + " " + use_str);

                                PalMethods.replacepalettes(Path.GetFullPath(use_str), basepal, i,config_list);
                                //Console.WriteLine("palrep "+i);
                                //Console.WriteLine(basepal.palcnt + " " + basepal.Lpals.Count + " " + basepal.Rpals.Count);
                            }
                        }
                        else
                        {
                            Console.WriteLine("propogating empty line " + i + " " + pspaldirtextlist[i]);
                            if (i > basepal.Lpals.Count)
                            {
                                Palette newpal = new Palette();

                                basepal.Lpals.Add(newpal);
                            }

                            if (i > basepal.Rpals.Count)
                            {
                                Palette newpal = new Palette();

                                basepal.Rpals.Add(newpal);
                            }
                        }
                    }
                }
                else
                {

                    PalMethods.replacepalettes(PSPALdir,basepal);

                    //PalMethods.replacepalette(txtinpal, txtinpal, basepal, i);
                }

                foreach (string path in outputdirs)
                {
                    PalMethods.createfile(Path.Combine(workingdir, path), basepal.getdata(type,(pal_game)output_type));
                    
                }
            }
        }
    }
}
