using FarsiTypeNet;
using System.Diagnostics;

namespace FarsiTypeTest
{
    

    

    internal static class Program
    {
        

        

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string test = "سلام دنیا hello world hastam چطوری خوبی؟ ablah? حتما باید رو یک متن bozorg testesh konamm man    همین.";
            char character = FarsiType.GetChar(FarsiWord.FA_ALEF_MAD_ABOVE);
            string b = FarsiType.GetFarsiGlyph(test);
            Console.WriteLine(b);
        }

    }
}