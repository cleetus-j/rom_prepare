using System;
using System.IO;


namespace rom_prepare
{
    class Program
    {
        static void Main(string[] args)
           
        {
            int job_type = Convert.ToUInt16(args[0]);
            if (job_type==0)
            {
                var fileStream = new FileStream(args[1], FileMode.Open, FileAccess.Read);
                var streamReader = new StreamReader(fileStream);


                    string line;
                byte[] FileExportable = new byte[8000000];
                byte[] FileGoesIn = File.ReadAllBytes(args[1]);
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] line_par = line.Split(" ");
                    int JobTypeFromFile = Convert.ToInt16(line_par[0]);
                    string FileNameFromFile = line_par[1];
                    if (JobTypeFromFile==1)
                    {
                        Console.WriteLine("Padding file with x-megabits.");
                        string FileOut = "donepbm"+FileNameFromFile;
                        UInt32 PadWithMegaBits = Convert.ToUInt32(line_par[2]);
                        int FillType = Convert.ToUInt16(line_par[3]);
                        if (line_par[4]=="1")
                        {
                            line_par[4] = "true";
                        }
                        else
                        {
                            line_par[4] = "false";
                        }
                        bool SwapNeeded = Convert.ToBoolean(line_par[4]);
                        FileExportable = pad_rom_per_megabit(FileNameFromFile, PadWithMegaBits, FillType, SwapNeeded);
                        using (BinaryWriter w_h = new BinaryWriter(File.Open(FileOut, FileMode.Create)))
                        {
                            for (int i = 0; i < FileExportable.Length; i++)
                            {
                                w_h.Write(FileExportable[i]);
                            }

                        }


                    }
                    else if (JobTypeFromFile==2)
                    {
                        string FileOut = "donepbb" + FileNameFromFile;
                        UInt32 PadWithMegaBits = Convert.ToUInt32(line_par[2]);
                        int FillType = Convert.ToUInt16(line_par[3]);
                        if (line_par[4] == "1")
                        {
                            line_par[4] = "true";
                        }
                        else
                        {
                            line_par[4] = "false";
                        }
                        bool SwapNeeded = Convert.ToBoolean(line_par[4]);
                        FileExportable = pad_rom(FileNameFromFile, PadWithMegaBits, FillType, SwapNeeded);
                        using (BinaryWriter w_h = new BinaryWriter(File.Open(FileOut, FileMode.Create)))
                        {
                            for (int i = 0; i < FileExportable.Length; i++)
                            {
                                w_h.Write(FileExportable[i]);
                            }

                        }

                        Console.WriteLine("Padding file with x-bytes.");
                    }
                    else if (JobTypeFromFile==3)
                    {
                        string FileOut = "doneswp" + FileNameFromFile;
                        FileExportable=swap_rom(FileNameFromFile);
                        using (BinaryWriter w_h = new BinaryWriter(File.Open(FileOut, FileMode.Create)))
                        {
                            for (int i = 0; i < FileExportable.Length; i++)
                            {
                                w_h.Write(FileExportable[i]);
                            }

                        }

                        Console.WriteLine("Swapping ROM.");
                    }
                    else if (JobTypeFromFile==4)
                    {
                        rom_prepare_hilo(FileNameFromFile);
                        Console.WriteLine("Preparing ROM for 8-bit chips.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid job type.");
                        System.Environment.Exit(1);
                    }
                    Console.WriteLine(JobTypeFromFile);
                    Console.WriteLine(FileNameFromFile);
                        //System.Console.WriteLine("Whole line:" + line);
                        //foreach (var item in line_par)
                        //{
                        //    Console.WriteLine(item);
                        //}
                    }

                

            }
        }

        public static byte[] pad_rom_per_megabit(string file_in, UInt32 megabits,int fill_type,bool swap)
        {
            /*0-->00
             * 1-->255
             * 2-->random
             * Minden más-->00
             */
            //Input paraméterek üresek, vagy úgy egyáltalán nem jók.
            bool SwapNeeded = swap;
            try
            {
                string fn_in = Convert.ToString(file_in);
                string mbits = Convert.ToString(megabits);
                string f_t = Convert.ToString(fill_type);
                string swp = Convert.ToString(swap);
                if (string.IsNullOrWhiteSpace(swp)||string.IsNullOrWhiteSpace(f_t)||string.IsNullOrWhiteSpace(mbits)||string.IsNullOrWhiteSpace(fn_in))
                {
                    Console.WriteLine("An input variable is null,empty,or it's a whitespace.\nPlease check it and try again.");
                    System.Environment.Exit(-2);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Fatal error while checking input parameters, exiting program...");
                System.Environment.Exit(-3);
                throw;
            }
            //Ha a file üres.
            if (new FileInfo(file_in).Length == 0)
            {
                Console.WriteLine("The input file is empty. Please check it again. Exiting...");
                System.Environment.Exit(-4);
                // empty
            }

            byte[] rom_in = File.ReadAllBytes(file_in);
            var rom_size = file_in.Length;
            var target_size = megabits * 131072;
            bool target_size_check=true;
            //Ha a ROM célmérete kisebb mint az eredeti ROM.
            if (rom_size>target_size||megabits<=0)
            {
                Console.WriteLine("Target size is smaller than the ROM size or invalid megabits given, please check it.\n");
                target_size_check = false;
            }
            if (target_size_check==false)
            {
                Console.WriteLine("There was an issue during conversion, see the above message. Exiting...");
                System.Environment.Exit(-1);
            }
            UInt32 difference = Convert.ToUInt32(target_size - rom_size);
            byte[] diff_array = new byte[difference];
            //A ROM paddolását mivel végezzük?
            if (fill_type==0)
            {
                for (int i = 0; i < difference; i++)
                {
                    diff_array[i] = 0;
                }
            }
            else if (fill_type==1)
            {
                for (int i = 0; i < difference; i++)
                {
                    diff_array[i] = 255;
                }
            }
            //Ha nem nulla vagy egy, akkor véletlenszerű értékkel töltse fel.
            else

            {
                Random r = new Random();
                for (int i = 0; i < difference; i++)
                {
                    diff_array[i] = Convert.ToByte(r.Next(0, 255));
                }
            }

           // byte[] fill_size = new byte[megabits * 125000];
            byte[] rom_out=new byte[target_size];//Kimeneti tömb, aminek a mérete a kért értékre mutat majd.
            Array.Copy(rom_in, rom_out, rom_in.Length);//A bemenetet átmásoljuk az átmeneti tömbre.
            var fill_array_pos = 0;
            for (int i = rom_in.Length; i < rom_out.Length; i++)
            {
                rom_out[i] = diff_array[fill_array_pos];
                fill_array_pos++;
            }

            // Array.ConstrainedCopy(diff_array, 0, rom_out, file_in.Length, (int)difference);//Az újonnan generált értékeket kimásoljuk a kimeneti tömbre.
            //Kell-e a bájtfordítás.

            if (SwapNeeded == true)
            {
                // if array is odd we set limit to a.Length - 1.
                int limit = rom_out.Length - (rom_out.Length % 2);
                if (limit < 1) throw new Exception("array too small to be swapped.");
                for (int i = 0; i < limit - 1; i = i + 2)
                {
                    byte temp = rom_out[i];
                    rom_out[i] = rom_out[i + 1];
                    rom_out[i + 1] = temp;
                }

                //Array.Reverse(rom_out);
            }

            return rom_out;

        }
        public static byte[] pad_rom(string file_in, UInt32 pad_bytes,int fill_type,bool swap)
        {
            /*Ebből kell két példány, egy ami megabitekre kerekít, és egy, ami megadott méretre kerekít.
             * Egy harmadik opció, hogy a program 00,FF vagy véletlenszerű adattal tölti fel a maradék részeket.
             * Annyira nem látom értelmét, de hátha máskor meg hasznos lesz.
             *
             */
            bool SwapNeeded = swap;
            if (fill_type>2)
            {
                Console.WriteLine("The provided fill type for the ROM expansion is invalid.\nOnly 0-(zeroes),1-(255\\FF),2-random is supported.");
                System.Environment.Exit(-5);
            }
            byte fill;
            byte[] fill_array = new byte[pad_bytes];
            if (fill_type==0)
            {
                fill = 0;
                for (int i = 0; i < pad_bytes; i++)
                {
                    fill_array[i] = fill;
                }
            }
            else if (fill_type==1)
            {
                fill = 255;
                for (int i = 0; i < pad_bytes; i++)
                {
                    fill_array[i] = fill;
                }
            }
            else
            {
                Random r = new Random();
                for (int i = 0; i < pad_bytes; i++)
                {
                    fill = Convert.ToByte(r.Next(0, 255));
                    fill_array[i] = fill;
                }
                

            }
            //Ide UInt32 kell, mert nincsenek negatív fileméretek.
            byte[] rom_in = File.ReadAllBytes(file_in);//Bemeneti rom ideiglenes tárolóhelye.
            UInt32 full_length = Convert.ToUInt32(rom_in.Length) + pad_bytes;//A teljes fileméret és amennyivel paddolni akarjuk.

            byte[] rom_out = new byte[full_length];//A kimenet méretének meghatározása.
            var fill_array_pos = 0;
            Array.Copy(rom_in, rom_out, rom_in.Length);//A bemenetet a kimenetre másoljuk.
            for (int i = rom_in.Length; i < rom_in.Length; i++)
            {
                rom_out[i] = fill_array[fill_array_pos];
                fill_array_pos++;
            }
            //Array.ConstrainedCopy(fill_array, 0, rom_out, file_in.Length, fill_array.Length);//A generált tömböt a kimenetre másoljuk, vagyis oda, ahol a rom véget ér.
            if (SwapNeeded==true)//Ha kell, akkor legyen byteswap.
            {
                // if array is odd we set limit to a.Length - 1.
                int limit = rom_out.Length - (rom_out.Length % 2);
                if (limit < 1) throw new Exception("array too small to be swapped.");
                for (int i = 0; i < limit - 1; i = i + 2)
                {
                    byte temp = rom_out[i];
                    rom_out[i] = rom_out[i + 1];
                    rom_out[i + 1] = temp;
                }


            }
            return rom_out;
        }
        public static byte[] swap_rom(string file_in)
        {
            byte[] rom_out = File.ReadAllBytes(file_in);//Bemeneti rom ideiglenes tárolóhelye.

            // if array is odd we set limit to a.Length - 1.
            int limit = rom_out.Length - (rom_out.Length % 2);
            if (limit < 1) throw new Exception("array too small to be swapped.");
            for (int i = 0; i < limit - 1; i = i + 2)
            {
                byte temp = rom_out[i];
                rom_out[i] = rom_out[i + 1];
                rom_out[i + 1] = temp;
            }

            return rom_out;
        }
        public static void rom_prepare_hilo(string file_in)
        {
            byte[] rom_in = System.IO.File.ReadAllBytes(file_in);//Beolvassuk a file-t.

            // if array is odd we set limit to a.Length - 1.
            int limit = rom_in.Length - (rom_in.Length % 2);
            if (limit < 1) throw new Exception("array too small to be swapped.");
            for (int i = 0; i < limit - 1; i = i + 2)
            {
                byte temp = rom_in[i];
                rom_in[i] = rom_in[i + 1];
                rom_in[i + 1] = temp;
            }
            int rom_high_pos = 0;
            int rom_low_pos = 0;
            String low_byte = "_low" + file_in;
            String high_byte = "_high" + file_in;
            byte[] rom_high = new byte[rom_in.Length];
            byte[] rom_low = new byte[rom_in.Length];

            for (int i = 0; i < rom_in.Length;)
            {
                rom_high[rom_high_pos] = rom_in[i];
                i++;
                rom_low[rom_low_pos] = rom_in[i];
                i++;
                rom_high_pos++;
                rom_low_pos++;

            }
            using (BinaryWriter w_h = new BinaryWriter(File.Open(high_byte, FileMode.Create)))
            {
                for (int i = 0; i < rom_high_pos; i++)
                {
                    w_h.Write(rom_high[i]);
                }

            }
            using (BinaryWriter w_l = new BinaryWriter(File.Open(low_byte, FileMode.Create)))
            {
                for (int i = 0; i < rom_low_pos; i++)
                {
                    w_l.Write(rom_low[i]);
                }


            }


        }
    }
}
