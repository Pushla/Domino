using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoC
{
    class MTable
    {
        // количество доминошек в руке в начале игры
        public const int conStartCount = 7;
        // Состояние игры
        public enum EFinish {Play=0, First, Second, Lockdown}; // играем, выиграл первый, выиграл второй, рыба

        // Доминушка
        public struct SBone
        {
            public ushort First;
            public ushort Second;

            public void Exchange()
            {
                ushort shrTemp = First;
                First = Second;
                Second= shrTemp;
            }
        }

        // "Базар" доминошек
        static private List<SBone> lBoneyard;
        //Текущий расклад на столе
        static private List<SBone> lGame;
        // Номер хода игры
        static private int intGameStep =1;
        // Количество взятых доминушек игроком за прошлый \ текущий ход 
        static private int intLastTaken, intTaken;
        // Генератор случайных чисел
        static private Random rnd;


        //***********************************************************************
        // Инициализация игры
        //***********************************************************************
        static private void Initialize()
        {
            SBone sb;

            rnd = new Random();
            // Очищаем коллекции в этом модуле
            lBoneyard = new List<SBone>();
            lGame = new List<SBone>();
        
            // Формирование базара
            for (ushort shrC = 0; shrC<=6; shrC++)
                for (ushort shrB = shrC; shrB<=6; shrB++)
                {
                    sb.First = shrC;
                    sb.Second = shrB;
                    lBoneyard.Add(sb);
                }

            // Инициализация игроков
            MFPlayer.Initialize();
            MSPlayer.Initialize();
        }

        //***********************************************************************
        // Получение случайной доминошки (sb) из базара
        // Возвращает FALSE, если базар пустой
        //***********************************************************************
        static public bool GetFromShop(out SBone sb)
        {
            int intN;
            sb.First = 7; sb.Second = 7;
        
            if (lBoneyard.Count == 0) return false;

            // Подсчет количества взятых доминушек одним игроком за текущий ход
            intTaken += 1;
            // определяем случайным образом доминушку из базара
            intN = rnd.Next(lBoneyard.Count -1);
            sb = lBoneyard[intN];
            // удаляем ее из базара
            lBoneyard.RemoveAt(intN);
            Console.WriteLine("Взяли из базара: " + sb.First + ":" + sb.Second + " ");
            return true;
        }

        //***********************************************************************
        // Возвращает количество оставшихся доминошек в базаре
        //***********************************************************************
        static public int GetShopCount()
        { 
            return lBoneyard.Count;
        }

        //***********************************************************************
        // Возвращает количество взятых игроком доминушек за текущий ход
        //***********************************************************************
        static public int GetTaken()
        {return intLastTaken;}

        //***********************************************************************
        // Возвращает информацию о текущем раскладе на столе
        //***********************************************************************
        static public List<SBone> GetGameCollection()
        { return lGame.ToList();  }

        //***********************************************************************
        // Раздача доминошек обоим игрокам в начале игры
        //***********************************************************************
        static public void GetHands()
        {
            SBone sb;
            for(int intC = 0 ; intC < conStartCount; intC++)
            {
                if (GetFromShop(out sb)) MFPlayer.AddItem(sb);
                intTaken = 0;
                if (GetFromShop(out sb)) MSPlayer.AddItem(sb);
                intTaken = 0;
            }
       }

        //***********************************************************************
        // Вывод на экран всех элементов коллекции colX
        //***********************************************************************
        static public void PrintAll(List<SBone> lItems)
        {
            foreach(SBone sb in lItems)
                Console.Write(sb.First + ":" + sb.Second + "  ");
            Console.WriteLine();
        }

        //***********************************************************************
        // Положить доминушку на стол
        //***********************************************************************
        static private bool SetBone(SBone sb, bool blnEnd)
        {
            SBone sbT;
            if (blnEnd)
            {
                sbT = lGame[lGame.Count - 1];
                if(sbT.Second == sb.First)
                {   
                    lGame.Add(sb);
                    return true;
                }
                else if (sbT.Second == sb.Second)
                {
                    sb.Exchange();
                    lGame.Add(sb);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                sbT = lGame[0];
                if(sbT.First == sb.Second)
                {   
                    lGame.Insert(0, sb);
                    return true;
                }
                else if (sbT.First == sb.First)
                {
                    sb.Exchange();
                    lGame.Insert(0, sb);
                    return true;
                }
                else
                    return false;
            }
        }

        static void Main(string[] args)
        {
            // кто сейчас ходит
            bool blnFirst;
            // результат текущего хода игроков =TRUE, если ход состоялся
            bool blnFRes, blnSRes;
            // признак окончания игры
            EFinish efFinish = EFinish.Play;
            // сообщения о результате игры
            string[] arrFinishMsg = {"---", "Победил первый игрок!", "Победил второй игрок!", "Рыба!"};
            // количество доминушек в базаре, нужно для определения корректности хода игрока
            int intBoneyard =0;
            // Чем ходить
            SBone sb;
            // куда ходить
            bool blnEnd;
    
            // Инициализация игры
            Initialize();
            // Раздача доминошек в начале игры
            GetHands();
            // первая доминушка - первая из базара
            // определяем случайным образом доминушку из базара
            int intN = rnd.Next(lBoneyard.Count - 1);
            lGame.Add(lBoneyard[intN]);
            lBoneyard.RemoveAt(intN);
            // вывод на экран начального состояния игры
            Console.WriteLine("*************ИГРА НАЧАЛАСЬ*********************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("*************Шаг #0");
            Console.ForegroundColor = ConsoleColor.White;
            PrintAll(lGame);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ИГРОК " + MFPlayer.PlayerName);
            MFPlayer.PrintAll();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("ИГРОК " + MSPlayer.PlayerName);
            MSPlayer.PrintAll();
            Console.ReadKey();

            blnFRes = true;
            blnSRes = true;
            // Первым ходит первый игрок
            blnFirst = false;
        
            intBoneyard = lBoneyard.Count;
            //-----------------------------------------------------------------
            // ИГРА
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.White;

                // кто ходит? ---- Ходит первый игрок
                if (blnFirst)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("*************Шаг #" + intGameStep + " " + MFPlayer.PlayerName);
                    Console.ForegroundColor = ConsoleColor.White;
                    // количество взятых доминушек
                    intLastTaken = intTaken;
                    intTaken = 0;
                    // ход первого игрока
                    intBoneyard = lBoneyard.Count;
                    blnFRes = MFPlayer.MakeStep(out sb, out blnEnd);
                    // если ход сделан
                    if (blnFRes)
                    {
                        // пристраиваем доминушку
                        if (SetBone(sb, blnEnd) == false)
                        {
                            Console.WriteLine("!!!!!!!!Жульничаем!!!!!! " + MFPlayer.PlayerName);
                            Console.ReadLine();
                            return;
                        }
                    }
                    // если ход не сделан
                    else if(intBoneyard == lBoneyard.Count && intBoneyard > 0)
                    {
                        Console.WriteLine("!!!!!!!!Жульничаем!!!!!! " + MFPlayer.PlayerName);
                        Console.ReadLine();
                        return;
                    }

                    if (blnFRes == false && blnSRes == false)
                    // рыба
                        efFinish = EFinish.Lockdown;
                    else if (blnFRes == true)
                        // если нет домино, то я выиграл
                        if (MFPlayer.GetCount() == 0) efFinish = EFinish.First;
                }
                // кто ходит? ---- Ходит вторый игрок
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("*************Шаг #" + intGameStep + " " + MSPlayer.PlayerName);
                    Console.ForegroundColor = ConsoleColor.White;

                    // количество взятых доминушек
                    intLastTaken = intTaken;
                    intTaken = 0;
                    // ход первого игрока
                    intBoneyard = lBoneyard.Count;
                    blnSRes = MSPlayer.MakeStep(out sb, out blnEnd);
                    // если ход сделан
                    if (blnSRes)
                    {
                        // пристраиваем доминушку
                        if (SetBone(sb, blnEnd) == false)
                        {
                            Console.WriteLine("!!!!!!!!Жульничаем!!!!!! " + MSPlayer.PlayerName);
                            Console.ReadLine();
                            return;
                        }
                    }
                        // если ход не сделан
                    else if(intBoneyard == lBoneyard.Count && intBoneyard > 0)
                    {
                        Console.WriteLine("!!!!!!!!Жульничаем!!!!!! " + MSPlayer.PlayerName);
                        Console.ReadLine();
                        return;
                    }

                    if (blnFRes == false && blnSRes == false)
                        // рыба
                        efFinish = EFinish.Lockdown;
                    else if (blnSRes == true)
                        // если нет домино, то я выиграл
                        if (MSPlayer.GetCount() == 0) efFinish = EFinish.First;
                }
            // после хода вывести данные на столе--------------------------------------------------------
            PrintAll(lGame);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ИГРОК " + MFPlayer.PlayerName);
            MFPlayer.PrintAll();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("ИГРОК " + MSPlayer.PlayerName);
            MSPlayer.PrintAll();
            //Console.ReadKey()
            // будет ходить другой игрок
            blnFirst = ! blnFirst;
            intBoneyard = lBoneyard.Count;
            intGameStep += 1;
        }
        while(efFinish == EFinish.Play);
        // результат текущей игры
        Console.WriteLine(arrFinishMsg[(int) efFinish]);
        Console.WriteLine("СЧЕТ -- " + MFPlayer.GetScore() + ":" + MSPlayer.GetScore());
        Console.ReadLine();
        }
    }
}
