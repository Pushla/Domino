static public bool MakeStep(out MTable.SBone sb, out bool End)
        {
            End = false; sb = new MTable.SBone(); //Инициализация переменных на вывод

            List<MTable.SBone> lstGame = MTable.GetGameCollection();

            //Узнать какие крайние значения на доске
            int borderLeft = lstGame[0].First;
            int borderRight = lstGame[lstGame.Count - 1].Second;

            int[] outerValues = { Math.Max(borderLeft, borderRight),
                Math.Min(borderLeft, borderRight) }; //Массив крайних значений для удобного формулирования условий на Max/Min 
            List<MTable.SBone> lstPossibleMoves = new List<MTable.SBone>(); //Все кости которые можно выложить

            //==============================================================================================
            //Добавить все возможные варианты костей для хода в лист
            do
            {
                foreach (MTable.SBone boneNew in lstHand)
                {
                    if (BoneContainsNum(boneNew, borderRight) || BoneContainsNum(boneNew, borderLeft))
                    {
                        lstPossibleMoves.Add(boneNew);
                    }
                }

                if (lstPossibleMoves.Count == 0)
                //Если ничего не было добавлено, вариантов для хода нет и надо взять кость из базара
                {
                    MTable.SBone addBone;
                    if (!MTable.GetFromShop(out addBone)) return false; //Игрок не может взять из пустого базара и не может сделать ход
                    AddItem(addBone);
                }
            } while (lstPossibleMoves.Count == 0);
            //==============================================================================================


            //Далее возможные варианты хода (От наименее вероятных - к наиболее вероятным):
            //1. В руке всего одна карта, которой можно воспользоваться
            //2. На руке есть как минимум один дубль
            //3. На краях выложенной цепочки одинаковые значения
            //4. В руке есть одно из или сразу оба крайних значения

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //1.
            if (lstPossibleMoves.Count == 1)
            //Для всех последующщих проверок предполагается, что возможных фишек хотя бы две
            {
                
                sb = lstPossibleMoves[0];
                if (BoneContainsNum(sb, borderLeft)) //Если кость содержит крайнее значение слева, добавить её в начало
                    End = false;
                else
                    End = true;

                lstHand.Remove(sb);
                return true;
            }
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            /*************************************************************************************************************/
            //2.
            MTable.SBone sbMaxDup = new MTable.SBone(); sbMaxDup.First = 7;
            bool dupExists = false;

            foreach (MTable.SBone b in lstPossibleMoves)
            //На случай, если дублей, которыми можно походить несколько - выложить кость с наибольшими значениями
            {
                if (b.First == b.Second && (sbMaxDup.First < b.First || sbMaxDup.First > 6))
                {
                    sbMaxDup = b;
                    dupExists = true;
                    if (b.First == 0) break;
                }
            }
            if (dupExists) //В руке был найден дубль
            {
                int compare = -1;
                if (BoneContainsNum(sbMaxDup, 0)) //Если есть возможность избавиться от дубля с нулями, сделать это в первую очередь, т.к. при проигрыше он стоит 25 баллов
                {
                    compare = 0;
                }
                else if (BoneContainsNum(sbMaxDup, outerValues[0])) //Дубль содержит максимальное крайнее значение
                {
                    compare = outerValues[0];
                }
                else if (BoneContainsNum(sbMaxDup, outerValues[1]))
                {
                    compare = outerValues[1];
                }


                sb = sbMaxDup;
                if (borderLeft == compare) //Добавить кость в начало или в конец
                    End = false;
                else
                    End = true;

                lstHand.Remove(sb);
                return true;
            }
            /*************************************************************************************************************/

            //________________________________________________________________
            //3.
            if (borderLeft == borderRight)
            //Далее предполагается, что в руке нет дублей после предыдущей проверки и существует как минимум одно верное решение
            {
                sb = MaxBoneInList(lstPossibleMoves);
                End = false; //Добавить кость куда-нибудь

                lstHand.Remove(sb);
                return true;
            }
            //________________________________________________________________


            //4.
            //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            //Сделать проверку, есть ли в руке оба крайних значения
            bool left = false, right = false;
            foreach (MTable.SBone b in lstPossibleMoves)
            {
                if (BoneContainsNum(b, borderLeft)) left = true;
                if (BoneContainsNum(b, borderRight)) right = true;

                if (right || left) break;
            }

            if (right || left)
            //Разбить кости на две категории по масти. Предполагается что после прошлой проверки на крайних костях значения не одинаковые
            {
                MTable.SBone suitableBone = new MTable.SBone();
                bool sBChosen = false;
                List<MTable.SBone> leftBones = new List<MTable.SBone>(); //Кости, которые можно подставить налево
                List<MTable.SBone> rightBones = new List<MTable.SBone>(); //Кости, которые можно подставить направо

                foreach (MTable.SBone b in lstPossibleMoves)
                {
                    int[] boneValues = { b.First, b.Second };
                    if (outerValues.Contains(boneValues[0]) && outerValues.Contains(boneValues[1]))
                    //Фишка подходит и под левый и под правый концы цепи
                    {
                        suitableBone = b;
                        sBChosen = true;
                    }
                    else if (boneValues.Contains(borderLeft))
                    {
                        leftBones.Add(b);
                    }
                    else if (boneValues.Contains(borderRight))
                    {
                        rightBones.Add(b);
                    }
                }

                if (!sBChosen)
                //Если нет кости, которая подошла бы под оба конца - выбрать крайнее значение с наибольшим числом вариантов и найти там максимальное
                {
                    if (leftBones.Count > rightBones.Count)
                    {
                        End = false;
                        suitableBone = MaxBoneInList(leftBones); //Выбрать из имеющихся костей кость с наибольшей суммой
                    }
                    else
                    {
                        End = true;
                        suitableBone = MaxBoneInList(rightBones); //Выбрать из имеющихся костей кость с наибольшей суммой
                    }
                }
                else
                //Есть кость, которая подошла бы под оба конца
                {
                    //Костей, которые подходят под значение слева больше, чем справа ->
                    // -> Надо поставить кость справа, чтобы оба значения стали равны левому
                    if (leftBones.Count > rightBones.Count)
                        End = true;
                    else
                        End = false;
                }

                sb = suitableBone;
                lstHand.Remove(sb);
                return true;
            }
            //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


            return false;
        }
