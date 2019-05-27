using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;

namespace MinecraftMapArtGenerator
{
    class Program
    {

        private static readonly Dictionary<string, Color> MapColors = new Dictionary<string, Color>
        {
            { "slime_block", Color.FromArgb(127, 178, 56) },
            { "sandstone", Color.FromArgb(247, 233, 163) },
            { "mushroom_stem", Color.FromArgb(199, 199, 199) },
            { "redstone_block", Color.FromArgb(255, 0, 0) },
            { "packed_ice", Color.FromArgb(160, 160, 255) },
            { "iron_block", Color.FromArgb(167, 167, 167) },
            { "oak_leaves", Color.FromArgb(0, 124, 0) },
            { "white_wool", Color.FromArgb(255, 255, 255) },
            { "clay", Color.FromArgb(164, 168, 184) },
            { "jungle_wood", Color.FromArgb(151, 109, 77) },
            { "cobblestone", Color.FromArgb(112, 112, 112) },
            //{ "minecraft:water", Color.FromArgb(64, 64, 255) },
            { "oak_wood", Color.FromArgb(143, 119, 72) },
            { "quartz_block", Color.FromArgb(255, 252, 245) },
            { "acacia_planks", Color.FromArgb(216, 127, 51) },
            { "purpur_block", Color.FromArgb(178, 76, 216) },
            { "light_blue_wool", Color.FromArgb(102, 153, 216) },
            { "hay_block", Color.FromArgb(229, 229, 51) },
            { "lime_wool", Color.FromArgb(127, 204, 25) },
            { "pink_wool", Color.FromArgb(242, 127, 165) },
            { "gray_wool", Color.FromArgb(65, 65, 65) },
            { "light_gray_wool", Color.FromArgb(153, 153, 153) },
            { "cyan_wool", Color.FromArgb(76, 127, 153) },
            { "purple_wool", Color.FromArgb(127, 63, 178) },
            { "blue_wool", Color.FromArgb(51, 76, 178) },
            { "dark_oak_wood", Color.FromArgb(102, 76, 51) },
            { "green_wool", Color.FromArgb(102, 127, 51) },
            { "bricks", Color.FromArgb(153, 51, 51) },
            { "black_wool", Color.FromArgb(21, 21, 21) },
            { "gold_block", Color.FromArgb(250, 238, 77) },
            { "diamond_block", Color.FromArgb(92, 219, 213) },
            { "lapis_block", Color.FromArgb(74, 128, 255) },
            { "emerald_block", Color.FromArgb(0, 217, 58) },
            { "spruce_wood", Color.FromArgb(129, 86, 49) },
            { "netherrack", Color.FromArgb(112, 2, 0) },
            { "white_terracotta", Color.FromArgb(209, 177, 161) },
            { "orange_terracotta", Color.FromArgb(159, 82, 36) },
            { "magenta_terracotta", Color.FromArgb(149, 87, 108) },
            { "light_blue_terracotta", Color.FromArgb(112, 108, 138) },
            { "yellow_terracotta", Color.FromArgb(186, 133, 36) },
            { "lime_terracotta", Color.FromArgb(103, 117, 53) },
            { "pink_terracotta", Color.FromArgb(160, 77, 78) },
            { "gray_terracotta", Color.FromArgb(57, 41, 35) },
            { "light_gray_terracotta", Color.FromArgb(135, 107, 98)},
            { "cyan_terracotta", Color.FromArgb(87, 92, 92) },
            { "purple_terracotta", Color.FromArgb(122, 73, 88) },
            { "blue_terracotta", Color.FromArgb(76, 62, 92) },
            { "brown_terracotta", Color.FromArgb(76, 50, 35) },
            { "green_terracotta", Color.FromArgb(76, 82, 42) },
            { "red_terracotta", Color.FromArgb(142, 60, 46) },
            { "black_terracotta", Color.FromArgb(37, 22, 16) },
        };

        private static List<string> Commands = new List<string>();
        //private static string DeleteCommand = "{id:\"minecraft:command_block_minecart\", Command:\"/fill ~-3 ~ ~ ~-2 ~ ~ minecraft:redstone_block\"}";
        private static string DeleteCommand = "{id:\"command_block_minecart\", Command:\"/kill @e[type=command_block_minecart,distance=..2]\"}";

        private static int index = 0;

        private static void CommandBuilder(string command)
        {
            if (Commands.Count == index) Commands.Add("/summon command_block_minecart ~ ~2 ~ {Command:\"\", Passengers:[");

            if (Commands[index].Length + command.Length + DeleteCommand.Length >= 30000)
            {
                Commands[index] += DeleteCommand + "]}";
                index++;
                Commands.Add("/summon command_block_minecart ~ ~2 ~ {Command:\"\", Passengers:[{id:\"command_block_minecart\", Command:\"" + command + "\"},");
            }
            else
            {
                Commands[index] += "{id:\"command_block_minecart\", Command:\"" + command + "\"},";
            }
        }

        // distance in RGB space
        private static int ColorDiff(Color c1, Color c2)
        {
            return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                  + (c1.G - c2.G) * (c1.G - c2.G)
                                  + (c1.B - c2.B) * (c1.B - c2.B));
        }

        private static double ColorDistance(Color c1, Color c2)
        {
            int red1 = c1.R;
            int red2 = c2.R;
            int rmean = (red1 + red2) >> 1;
            int r = red1 - red2;
            int g = c1.G - c2.G;
            int b = c1.B - c2.B;
            return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }

        private static Color ClosestColor(Dictionary<Color, Tuple<int, string>> colors, Color target)
        {
            Color? lowest = null;
            int diff = Int32.MaxValue;

            foreach (var color in colors.Keys)
            {
                int d = ColorDiff(color, target);
                if (d < diff)
                {
                    lowest = color;
                    diff = d;
                }
            }

            return lowest.Value;
        }

        private static Color ClosestColor2(Dictionary<Color, Tuple<int, string>> colors, Color target)
        {
            Color? lowest = null;
            double diff = Int32.MaxValue;

            foreach (var color in colors.Keys)
            {
                double d = ColorDistance(color, target);
                if (d < diff)
                {
                    lowest = color;
                    diff = d;
                }
            }

            return lowest.Value;
        }

        private static Color ColorCalc(Color color, int index)
        {
            int multiplier = 0;
            switch (index)
            {
                case -1:
                    multiplier = 180;
                    break;
                case 0:
                    multiplier = 220;
                    break;
                case 1:
                    multiplier = 255;
                    break;
                default:
                    throw new InvalidOperationException("index is invalid");

            }

            return Color.FromArgb((byte)((color.R * multiplier) / 255), (byte)((color.G * multiplier) / 255),
                (byte)((color.B * multiplier) / 255));
        }

        [STAThread]
        private static int Main(string[] args)
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

            if (args.Length != 1)
            {
                Console.Error.WriteLine($"Expected 1 argument, got {args.Length}.");
                return 1;
            }

            if (!File.Exists(args[0]))
            {
                Console.Error.WriteLine($"Input file \"{args[0]}\" does not exist.");
                return 1;
            }

            var actualColors = new Dictionary<Color, Tuple<int, string>>();

            foreach (var mapColor in MapColors)
            {
                for (int i = -1; i < 2; i++)
                {
                    actualColors.Add(ColorCalc(mapColor.Value, i), new Tuple<int, string>(i, mapColor.Key));
                }
            }


            var inputBitmap = new Bitmap(args[0]);

            var outputBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);

            for (int x = 0; x < inputBitmap.Width; x++)
            {
                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    Color color = ClosestColor(actualColors, inputBitmap.GetPixel(x, y));

                    outputBitmap.SetPixel(x, y, color);
                }
            }

            outputBitmap.Save("First.png", ImageFormat.Png);

            for (int x = 0; x < inputBitmap.Width; x++)
            {
                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    Color color = ClosestColor2(actualColors, inputBitmap.GetPixel(x, y));

                    outputBitmap.SetPixel(x, y, color);
                }
            }

            outputBitmap.Save("Second.png", ImageFormat.Png);

            var mostColorDict = new Dictionary<Color, int>();

            for (int x = 0; x < outputBitmap.Width; x++)
            {
                for (int y = 0; y < outputBitmap.Height; y++)
                {
                    Color color = outputBitmap.GetPixel(x, y);

                    if (!mostColorDict.ContainsKey(color))
                        mostColorDict.Add(color, 0);

                    mostColorDict[color] = mostColorDict[color] + 1;
                }
            }


            //Console.WriteLine($"Most used color is: {actualColors[color1.Value]}");
            //Console.WriteLine("Commands:\r\n");

            //Console.WriteLine($"/fill ~1 ~-1 ~1 ~128 ~-1 ~128 {actualColors[color1.Value].Item2}");

            for (int y = 0; y < outputBitmap.Height; y += 8)
            {
                for (int x = 0; x < outputBitmap.Width; x += 8)
                {
                    CommandBuilder($"/fill ~{x+1} 0 ~{y+1} ~{x + 8} 255 ~{y + 8} air");
                }
            }

            //for (int i = 0; i < 256; i++)
            //{
                //Console.WriteLine($"/fill ~1 {i} ~1 ~128 {i} ~128 minecraft:air");
            //    CommandBuilder($"/fill ~1 {i} ~1 ~{outputBitmap.Width} {i} ~{outputBitmap.Height} minecraft:air");
            //}

            var ypos = new int[outputBitmap.Width];

            for (int i = 0; i < ypos.Length; i++)
            {
                ypos[i] = 127;
            }

            var zpos = new int[outputBitmap.Width, outputBitmap.Height];

            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    Color col = outputBitmap.GetPixel(x, y);
                    int i = actualColors[col].Item1;
                    //ypos[x] = ypos[x] + i;

                    //if (ypos[x] <= 0 || ypos[x] >= 255)
                    //{
                    //    ypos[x] = 127;
                    //}
                    if (i > 0 && ypos[x] < 127)
                    {
                        ypos[x] = 127;
                    }
                    else if (i < 0 && ypos[x] > 127)
                    {
                        ypos[x] = 127;
                    }
                    else
                    {
                        ypos[x] = ypos[x] + i;
                    }

                    zpos[x, y] = ypos[x];
                    //Console.WriteLine($"/setblock ~{x+1} {ypos[x]} ~{y+1} {actualColors[col].Item2}");
                    //CommandBuilder($"/setblock ~{x + 1} {ypos[x]} ~{y + 1} {actualColors[col].Item2}");
                }
            }

            int max = Int32.MinValue;
            int min = Int32.MaxValue;

            for (int y = 0; y < zpos.GetLength(1); y++)
            {
                for (int x = 0; x < zpos.GetLength(0); x++)
                {
                    if (zpos[x, y] > max) max = zpos[x, y];
                    if (zpos[x, y] < min) min = zpos[x, y];
                }
            }

            if (min < 0 && max < 256)
            {
                for (int x = 0; x < zpos.GetLength(0); x++)
                {
                    for (int y = 0; y < zpos.GetLength(1); y++)
                    {
                        zpos[x, y] += 255 - max;
                    }
                }
            }
            else if (max > 255 && min >= 0)
            {
                for (int x = 0; x < zpos.GetLength(0); x++)
                {
                    for (int y = 0; y < zpos.GetLength(1); y++)
                    {
                        zpos[x, y] -= min;
                    }
                }
            }


            for (int y = 0; y < outputBitmap.Height; y++)
            {
                for (int x = 0; x < outputBitmap.Width; x++)
                {
                    Color col = outputBitmap.GetPixel(x, y);
                    int i = actualColors[col].Item1;

                    //Console.WriteLine($"/setblock ~{x+1} {ypos[x]} ~{y+1} {actualColors[col].Item2}");
                    CommandBuilder($"/setblock ~{x + 1} {zpos[x,y]} ~{y + 1} {actualColors[col].Item2}");
                }
            }


            Commands[index] += DeleteCommand + "]}";

            foreach (var Command in Commands)
            {
                Console.WriteLine(Command);
            }

            //for (int y = 0; y < outputBitmap.Height; y++)
            //{
            //    for (int x = 0; x < outputBitmap.Width; x++)
            //    {
            //        if (outputBitmap.GetPixel(x, y) == color1.Value) continue;

            //        Color col = outputBitmap.GetPixel(x, y);

            //        if (x < 127 && outputBitmap.GetPixel(x + 1, y) == col)
            //        {
            //            int originalX = x;
            //            x += 2;
            //            int amount = 2;
            //            while (x < 127 && outputBitmap.GetPixel(x, y) == col)
            //            {
            //                amount++;
            //                x++;
            //            }

            //            x--;
            //            Console.WriteLine($"/fill ~{originalX} ~-1 ~{y} ~{x} ~-1 ~{y} {actualColors[col]}");
            //        }
            //        else
            //        {
            //            Console.WriteLine($"/setblock ~{x} ~-1 ~{y} {actualColors[outputBitmap.GetPixel(x, y)]}");
            //        }
            //    }
            //}

            //outputBitmap.Save(args[1], ImageFormat.Png);

            return 0;
        }
    }
}
