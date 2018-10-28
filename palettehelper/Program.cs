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
            string none = "null";

            /*
            string PSPALdir = Path.Combine(workingdir, none);
            string UNIPALdir = Path.Combine(workingdir, none);
            string outputdir = Path.Combine(workingdir, none);
            string txtlistdir = Path.Combine(workingdir, none);
            */

            string PSPALdir = "none";
            string UNIPALdir = "none";
            string outputdir = "none";
            string txtlistdir = "none";

            string palnumstring = "0";
            string alphacolorstring = "255";

            List<string> outputdirs = new List<string>();

            byte alphacolor = Convert.ToByte(alphacolorstring);

            bool bothsides = true;

            bool fromscratch = true;

            bool alphamode = false;

            int palnum = Convert.ToInt32(palnumstring);


            List<string> pspaldirtextlist = new List<string>();


            bool istxtlist = false;

            if (args.Length > 0)
            {
                int count = 0;
                foreach (string arg in args)
                {
                    Console.WriteLine(args.Length);
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
                            bothsides = Convert.ToBoolean(args[count + 1]);
                            break;
                        case "-basealpha":
                            alphacolor = Convert.ToByte(args[count + 1]);
                            break;
                    }
                    count++;
                    Console.WriteLine(count);
                }
            }
            else
            {
                Console.WriteLine("Syntax: -input MS.Pal|Pal List.txt|Pal.png|, -basepal UNI.Pal, -color Pal to replace, -output output file, -mode 'uniel/unist' mode to use, -sides 'true/false' enable/disable copy pasting to second side");
                System.Environment.Exit(0);
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

                        istxtlist = true;
                        break;
                    case 10:
                        //headeroff = 0;
                        break;
                }
                basepal = PalMethods.loadpals(basepalbytes);
                fromscratch = false;
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
                        if (File.Exists(pspaldirtextlist[i]) == false || (i > basepal.palcnt))
                        {
                            string mess = (i > basepal.palcnt) ? "file out of list/palette range" : "file not found";


                            Console.WriteLine("skipped " + pspaldirtextlist[i] +" num "+ i + " " + " because " + mess);
                        }
                        else
                        {
                            Console.WriteLine("cycling lines " + i + " " + pspaldirtextlist[i]);

                            PalMethods.replacepalettes(@pspaldirtextlist[i], basepal, i);
                            //Console.WriteLine("palrep "+i);
                            //Console.WriteLine(basepal.palcnt + " " + basepal.Lpals.Count + " " + basepal.Rpals.Count);
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
                    PalMethods.createfile(Path.Combine(workingdir, path), basepal.getdata(type));
                    
                }
            }

        }
       
    }
}
