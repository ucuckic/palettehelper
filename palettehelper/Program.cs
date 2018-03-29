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
        public static void Main(string[] args)
        {
            string workingdir = Directory.GetCurrentDirectory();
            string mode = "unist";
            string none = "null";
            string PSPALdir = Path.Combine(workingdir, none);
            string UNIPALdir = Path.Combine(workingdir, none);
            string outputdir = Path.Combine(workingdir, none);
            string txtlistdir = Path.Combine(workingdir, none);
            string palnumstring = "0";
            string alphacolorstring = "255";
            byte alphacolor = Convert.ToByte(alphacolorstring);

            bool bothsides = true;

            bool fromscratch = true;

            bool alphamode = false;

            if (args.Length > 0)
            {
                int count = 0;
                foreach (string arg in args)
                {
                    Console.WriteLine(args.Length);
                    switch (arg)
                    {
                        case "-input":
                            Console.WriteLine("test");
                            //System.Environment.Exit(0);
                            PSPALdir = Path.Combine(workingdir, @args[count + 1]);
                            break;
                        case "-basepal":
                            UNIPALdir = Path.Combine(workingdir, @args[count + 1]);
                            break;
                        case "-color":
                            palnumstring = args[count + 1];
                            break;
                        case "-output":
                            outputdir = Path.Combine(workingdir, @args[count + 1]);
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

            int palnum = Convert.ToInt32(palnumstring);

            try
            {
                string PSPALfn = Path.GetFileName(PSPALdir);
                //byte[] UNIPAL = File.ReadAllBytes(UNIPALdir);
                byte[] UNIPAL = new byte[86032];
                if ( File.Exists(UNIPALdir) )
                {
                    UNIPAL = File.ReadAllBytes(UNIPALdir);
                    fromscratch = false;
                }
                byte[] PSPAL = File.ReadAllBytes(PSPALdir);
                if(fromscratch)
                {
                }
                //byte[] workingpal = new byte[1024];
                //int UNIPALcount = UNIPAL[13];

                int headeroff = 15;

                if (UNIPAL[0] != 255)
                {
                    headeroff = 3;
                }
                if(fromscratch)
                {
                    headeroff = 15;
                }

                Console.WriteLine("brand new palette: "+fromscratch);
                //Console.WriteLine(headeroff);


                List<byte[]> header = new List<byte[]>();

                List<int> positions = new List<int>();

                List<byte[]> colorlist = new List<byte[]>();

                List<byte[]> palettelist = new List<byte[]>();


                List<string> filenamelist = new List<string>();


                int position = 0;
                int position2 = 0;
                int paloff = 0;
                Console.WriteLine(headeroff);
                //System.Environment.Exit(0);
                foreach (byte color in UNIPAL)
                {
                    //position++;
                    if (position > headeroff)
                    {
                        if (position2 == paloff)
                        {
                            //UNIPAL.CopyTo(workingpal, position);
                            //palettelist.Add(workingpal);
                            positions.Add(position);
                            //Array.Copy(UNIPAL, position, workingpal, 0, workingpal.Length);
                            paloff += 1024;
                        }
                        position2++;
                    }
                    position++;
                    //Console.WriteLine("foreach");
                }

                //byte[] workingpald = new byte[positions.Count*1024];

                //Array.Copy(UNIPAL, 0, workingpald, 0, workingpald.Length);
                //palettelist.Add(workingpald);


                foreach (int palpos in positions)
                {
                    byte[] workingpal = new byte[1024];
                    Array.Copy(UNIPAL, palpos, workingpal, 0, workingpal.Length);
                    palettelist.Add(workingpal);
                    //palettelist.Add(workingpal);
                    Console.WriteLine(palpos);
                    Console.WriteLine(workingpal.Length);
                    Console.WriteLine("loaded " + palettelist.Count + " palettes");
                }
                Console.WriteLine(palettelist.Count / 2 + " 'unique' palettes");

                Console.WriteLine("palnum " + palnum);

                if(palnum > palettelist.Count / 2)
                {
                    int oldpalnum = palnum;
                    palnum = palettelist.Count / 2 - 1;
                    Console.WriteLine("input color selection was too large ( "+oldpalnum+" ) and has been corrected to "+palnum);
                }



                //byte[] editingpal = palettelist.SelectMany(a => a).ToArray();


                //Console.WriteLine(PSPAL[0]);
                //Console.WriteLine(PSPAL[1]);
                //Console.WriteLine(PSPAL[2]);
                //Console.WriteLine(PSPAL[3]);

                int PLTEoff = 0;


                List<byte[]> pspallist = new List<byte[]>();

                bool istxtlist = false;

                List<string> pspaldirtextlist = new List<string>();

                List<string> pspaldirtextlist2 = new List<string>();

                List<byte[]> buildfile = new List<byte[]>();

                List<byte[]> editedpal = new List<byte[]>();

                List<byte> alphabytes = new List<byte>();

                List<byte> alphabytespos = new List<byte>();

                List<byte> alphabytescol = new List<byte>();

                int count = 0;
                int alphacount = 0;

                if (PSPALfn.EndsWith(".txt"))
                {
                    pspaldirtextlist = File.ReadAllLines(PSPALdir).ToList();
                    pspaldirtextlist2 = File.ReadAllLines(PSPALdir).ToList();
                    Console.WriteLine("textfile");
                    istxtlist = true;
                }

                int alphapos = 0;

                int index1 = 0;

                int index2 = 0;

                if (istxtlist)
                {

                    //string thingir = "";


                    Console.WriteLine("pos "+alphapos);

                    //if (alphamode)pspaldirtextlist.RemoveAt(alphapos);

                    foreach (string paldirp in pspaldirtextlist)
                    {

                        //index1 = paldirp.IndexOf("[") + 1;
                        index2 = paldirp.IndexOf(";");

                        string stringent = pspaldirtextlist[count];
                        
                        //Console.WriteLine("brap");
                        if (index2 != -1 )
                        {
                            stringent = pspaldirtextlist[count].Remove(index2, pspaldirtextlist[count].Length - index2);
                        }

                        Console.WriteLine(stringent);

                        string configdir = stringent + "_cfg.txt";

                        palnum = count;

                        if (File.Exists(configdir))
                        {
                            Console.WriteLine("config file found: "+configdir);
                            List<string> configtextlist = new List<string>();
                            configtextlist = File.ReadAllLines(configdir).ToList();
                            foreach (string line in configtextlist)
                            {
                                string fname = Path.GetFileName(configdir);

                                string processedline = line;

                                Match comments = Regex.Match(line, @"\/\/(.*)");

                                processedline = line.Remove(comments.Index, comments.Length);
                                
                                //Console.WriteLine(processedline);

                                Match input = Regex.Match(processedline, @"(^[^=]*)(\=)(.*)");

                                string paramstr = processedline.Substring(input.Groups[1].Index, input.Groups[1].Length);
                                string varstr = processedline.Substring(input.Groups[3].Index, input.Groups[3].Length);

                                //Console.WriteLine("parameter "+paramstr);
                                //Console.WriteLine("variable " + varstr);

                                switch (paramstr)
                                {
                                    case "test":
                                        //Console.WriteLine("test success");
                                        //Console.WriteLine("parameter " + paramstr + " help");
                                        //Console.WriteLine("variable " + varstr + " help");
                                        //System.Environment.Exit(0);
                                        //PSPALdir = Path.Combine(workingdir, @args[count + 1]);
                                        break;
                                    case "manualalpha":
                                        if (varstr.Contains("["))
                                        {
                                            index1 = varstr.IndexOf("[") + 1;
                                            index2 = varstr.IndexOf("]");
                                            string thingir = "";
                                            if (index1 != -1 && index2 != -1)
                                            {
                                                alphamode = true;
                                                thingir = varstr.Substring(index1, index2 - index1);
                                                //Console.WriteLine("somehow nothing is ok");
                                            }
                                            foreach (Match test1 in Regex.Matches(thingir, @"base=\d+"))
                                            {
                                                string da = thingir.Substring(test1.Index + 5, test1.Length - 5);

                                                Console.WriteLine(da);

                                                alphacolor = Convert.ToByte(da);

                                                thingir = thingir.Remove(test1.Index, test1.Length + 1);

                                                Console.WriteLine(thingir);

                                                Console.WriteLine(alphacolor);

                                                //System.Environment.Exit(0);

                                            }

                                            Console.WriteLine(thingir);
                                            Console.WriteLine(Regex.Matches(thingir, @"\d+").Count);
                                            foreach (Match test2 in Regex.Matches(thingir, @"\d+"))
                                            {
                                                string de = thingir.Substring(test2.Index, test2.Length);

                                                //Console.WriteLine("wowos " + de);

                                                alphabytes.Add(Convert.ToByte(de));
                                               //Console.WriteLine("wowowo");
                                                //Console.WriteLine(alphabytes.Count);
                                            }



                                            //Match test2 = Regex.Match(2).ToString;
                                            /*
                                            if (paldirp.StartsWith("Alpha:\"") && paldirp.EndsWith("\""))
                                            {
                                                string alphastring = paldirp.Substring(7, paldirp.Length - 8);
                                                alphacolor = Convert.ToByte(alphastring);

                                                alphamode = true;

                                                alphapos = alphacount;

                                                Console.WriteLine("alpha color overridden");
                                                Console.WriteLine("palnum " + palnum);

                                            }
                                            alphacount++;
                                            */

                                            alphacount++;

                                        }
                                        else
                                        {
                                            Console.WriteLine("was");
                                        }
                                        break;
                                    case "basealpha":
                                        alphacolor = Convert.ToByte(varstr);
                                        Console.WriteLine("alphacolor set to " + alphacolor + " in " + fname);
                                        break;
                                    case "palnum":
                                        palnum = Convert.ToInt32(varstr);
                                        Console.WriteLine("palnum set to " + palnum + " in " + fname);
                                        break;
                                    case "bothsides":
                                        int i2b = Convert.ToInt32(varstr);
                                        bothsides = Convert.ToBoolean(i2b);
                                        Console.WriteLine("bothsides set to " + bothsides + " in " + fname);
                                        break;
                                    default:
                                        Console.WriteLine("unrecognized config variable: " + "'" + processedline + "'");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if(stringent != "" ) Console.WriteLine("config file not found: "+configdir);
                        }
                        
                        if (File.Exists(stringent) == false || (palnum > palettelist.Count / 2 && bothsides) )
                        {
                            string mess = "thing";
                            mess = (palnum > palettelist.Count / 2) ? "file out of list/palette range" : "file not found";
                            Console.WriteLine(stringent);

                            count++;

                            Console.WriteLine("skipped " + palnum + " because " + mess);
                        }
                        else
                        {

                            Console.WriteLine(stringent + " help");

                            PSPAL = File.ReadAllBytes(stringent);

                            PSPALfn = Path.GetFileName(stringent);

                            Console.WriteLine(configdir + " help");


                            //System.Environment.Exit(0);

                            dofunc();

                            //palnum++;

                            count++;
                            //Console.WriteLine("count " + count);
                        }
                    }
                }
                else
                {
                    dofunc();
                }

                byte Pcnt = (byte)palettelist.Count;
                byte[] UNISTheader = new byte[] { 255, 255, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, Pcnt, 00, 00, 00 };

                byte[] UNIELheader = new byte[] { Pcnt, 00, 00, 00 };

                if (mode != "uniel" || mode == "unist")
                {
                    header.Add(UNISTheader);
                    Console.WriteLine("\nUNIST mode\n");
                }
                else if (mode == "uniel")
                {
                    header.Add(UNIELheader);
                    Console.WriteLine("\nUNIEL mode\n");
                }

                buildfile.InsertRange(0, palettelist);
                //buildfile.InsertRange(0, editedpal);
                buildfile.InsertRange(0, header);


                byte[] array = buildfile.SelectMany(a => a).ToArray();

                File.WriteAllBytes(outputdir, array);

                Console.WriteLine("written at " + outputdir);

                if(fromscratch) Console.WriteLine("\npalette generated without base .pal ( may have empty colors )\n");

                System.Environment.Exit(0);

                void dofunc()
                {

                //alphacolor = 255;
                
                bool ispng = false;
                
                List<byte> pngpalbytes = new List<byte>();

                byte[] editingpal = new byte[1024];

                byte[] RIFFheadercheck = new byte[] { 82, 73, 70, 70 };

                byte[] PNGPLTEcheck = new byte[] { 0x50, 0x4c, 0x54, 0x45 };

                byte[] PSPALheadercheck = new byte[] { PSPAL[0], PSPAL[1], PSPAL[2], PSPAL[3] };
                
                if ( PSPALfn.EndsWith(".png") )
                {
                    Console.WriteLine(".png extension");
                    byte[] bytes = new byte[4];
                    int pspalcnt = PSPAL.Length;
                    Console.WriteLine("pspallen "+pspalcnt);
                    for (int i = 0; i < pspalcnt-4; i++)
                    {
                        byte[] PSPALPNGPLTEcheck = new byte[] { PSPAL[i], PSPAL[i + 1], PSPAL[i + 2], PSPAL[i + 3] };
                        /*
                        Console.WriteLine(PSPAL[i]);
                        Console.WriteLine(PSPAL[i+1]);
                        Console.WriteLine(PSPAL[i+2]);
                        Console.WriteLine(PSPAL[i+3]);
                        */
                        //Console.WriteLine(PSPALPNGPLTEcheck.SequenceEqual(PNGPLTEcheck));
                        if (PSPALPNGPLTEcheck.SequenceEqual(PNGPLTEcheck) == true)
                        {
                            ispng = true;
                            PLTEoff = i + 4;
                            i = pspalcnt + 1;
                            Console.WriteLine(".png PLTE found");
                            Console.WriteLine("PLTEoff "+PLTEoff);
                        }
                        else
                        {
                            //Console.WriteLine("isnot");
                        }
                    }
                    if (ispng == false)
                    {
                        Console.WriteLine("could not find .png PLTE");
                        System.Environment.Exit(0);
                    }
                }
                if (ispng == true)
                {
                    int pos = 3;

                    int cpos = 1;

                    Array.Copy(PSPAL, PLTEoff, editingpal, 0, 768);
                    byte[] bobble = new byte[1024];
                    foreach (byte bote in editingpal)
                    {
                        byte ff = 0xff;
                        pngpalbytes.Add(bote);
                        if (cpos == pos)
                        {
                            pngpalbytes.Add(ff);
                            //Console.WriteLine("pos "+pos);
                            pos += 3;
                        }
                        else
                        {
                            //Console.WriteLine("nothing happening");
                        }
                        cpos++;
                    }
                    bobble = pngpalbytes.ToArray();
                    Console.WriteLine("pngpalbytes "+pngpalbytes.Count);
                    Console.WriteLine("bobble "+bobble.Length);
                    Array.Copy(bobble, 0, editingpal, 0, 1024);
                    Console.WriteLine("palette written from PLTE");
                }
                else
                if (PSPALheadercheck.SequenceEqual(RIFFheadercheck) == true)
                {
                    Array.Copy(PSPAL, 24, editingpal, 0, 1024);
                    Console.WriteLine("mspal header valid");
                }
                else if ( PSPALheadercheck[0] == 255 && ispng == false)
                {
                    Array.Copy(PSPAL, 16, editingpal, 0, 1024);
                    Console.WriteLine("not a ms pal probably unist");
                }
                else if ( PSPALheadercheck[0] != 255 && ispng == false)
                {
                    Array.Copy(PSPAL, 4, editingpal, 0, 1024);
                    Console.WriteLine("not a ms pal probably uniel");
                }
                else
                {
                    Console.WriteLine("somehow nothing is ok");
                }

                    //System.Environment.Exit(0);


                    // a lot of these probably arent used


                    //add color alpha override at some point in my lifetime
                    //probably done






                    string curstring = "none";

                    if (istxtlist) curstring = pspaldirtextlist2[palnum];

                    Match basematch = Regex.Match(curstring, @"base=\d+");

                    Console.WriteLine("we");





                    int colpos = 0;
                    int colpos2 = 3;
                    byte listcolpos = 0;
                    int listcolval = 0;
                    int basecoloff = 1;
                    if (alphamode)
                    {
                        for (int i = 1; i <= alphabytes.Count; i++)
                        {
                            if (i % 2 != 0)
                            {
                                alphabytespos.Add(alphabytes[i - 1]);
                               //Console.WriteLine(i+" posbytes");
                               //Console.WriteLine(alphabytes.Count);
                            }
                            else
                            {
                                alphabytescol.Add(alphabytes[i - 1]);
                               //Console.WriteLine(i + " colbytes");
                            }
                        }
                        colpos = 0;
                        colpos2 = 3;
                        listcolpos = alphabytespos[0];
                        listcolval = alphabytescol[0];
                        basecoloff = 1;
                    }

                
                if (istxtlist == false || istxtlist == true)
                {
                    foreach (byte colo in editingpal)
                    {
                        if (colpos == colpos2)
                        {
                            //Console.WriteLine(colpos);
                            editingpal[colpos] = alphacolor;
                            colpos2 += 4;
                        }
                        if(alphamode)
                        {
                            byte listcolposcnt = 0;
                            foreach (byte colthing in alphabytespos)
                            {
                                if (colpos == colthing * 4 - basecoloff)
                                {
                                    //Console.WriteLine("god " + alphabytespos.Count + " " +alphabytescol.Count);
                                    //Console.WriteLine(listcolposcnt);
                                    listcolpos = alphabytescol[listcolposcnt];
                                    editingpal[colpos] = listcolpos;
                                    basecoloff = 1;
                                }
                                listcolposcnt++;
                            }
                        }
                            //editingpal[colpos] = 255;
                            colpos++;
                        //Console.WriteLine(colo);
                    }
                }

                //Console.WriteLine("KABB");


                editedpal.Add(editingpal);

                int uniquecolorlist = palettelist.Count / 2;

                if (istxtlist == false || istxtlist==true)
                {
                        //palettelist.RemoveAt(palnum);
                        //palettelist.Insert(palnum, editingpal);

                    //int uniquecolorlist = palettelist.Count / 2;

                    if (bothsides == true && palnum < uniquecolorlist )
                    {
                        //palettelist.RemoveAt(palnum + uniquecolorlist);
                        //palettelist.Insert(palnum + uniquecolorlist, editingpal);
                        Console.WriteLine("both sides ok");
                    }
                    else
                    {
                        if (bothsides == false) Console.WriteLine("second side writing disabled");
                        else
                        {
                                bothsides = false;
                            Console.WriteLine("second side probably doesnt exist");
                        }
                    }
                    insert();
                }


                void insert()
                {
                    bool palexpandable = false;
                        if (palnum <= palettelist.Count && !palexpandable)
                        {
                            Console.WriteLine(palnum);
                            Console.WriteLine(palettelist.Count);
                            palettelist.RemoveAt(palnum);
                            Console.WriteLine(palettelist.Count);
                            palettelist.Insert(palnum, editingpal);
                            if (bothsides && palnum < uniquecolorlist + 1) Console.WriteLine("writing both sides");
                            if (bothsides && palnum < uniquecolorlist + 1) palettelist.RemoveAt(palnum + uniquecolorlist);
                            if (bothsides && palnum < uniquecolorlist + 1) palettelist.Insert(palnum + uniquecolorlist, editingpal);
                        }
                        else if (palnum >= palettelist.Count)
                        {
                            Console.WriteLine("writing both r");
                        }

                    //palettelist.Insert(palnum, editingpal);
                    //if(bothsides && palnum < uniquecolorlist + 1) palettelist.Insert(palnum + uniquecolorlist, editingpal);
                    //(bothsides && palnum < uniquecolorlist + 1) Console.WriteLine("writing both sides");
                    
                }

                    //palettelist.Add(editedpal[palnum]);

                    //editedpal.Add(workingpal);
                }

            }
            catch(FileNotFoundException FileNotFoundEx)
            {
                Console.WriteLine("base palette not supplied. " + FileNotFoundEx.Message);
            }
        }
    }
}
