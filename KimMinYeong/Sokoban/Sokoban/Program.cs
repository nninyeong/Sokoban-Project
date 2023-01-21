using Sokoban;
using System.Threading;
using System.Threading.Tasks;

namespace LetsMakeModules
{

    class Sokoban
    {
        static void Main()
        {
            // 초기 세팅
            Console.ResetColor(); // 컬러를 초기화 하는 것
            Console.CursorVisible = false; // 커서를 숨기기
            Console.Title = "Let's Make Sokoban"; // 타이틀을 설정한다.
            Console.BackgroundColor = ConsoleColor.DarkGreen; // 배경색을 설정한다.
            Console.ForegroundColor = ConsoleColor.Yellow; // 글꼴색을 설정한다.
            Console.Clear(); // 출력된 내용을 지운다.

            FrameTimer timer = new FrameTimer(5.0f);

            // 전체 게임의 흐름 (메모장 맵 툴 이용하기)
            // 1. 스테이지 파일 불러오기
            // LoadStage 함수 만들기

            string[] lines = Game.LoadStage(1);

            // 불러온 파일을 한줄씩 출력
            for (int i = 0; i < lines.Length; ++i)
            {
                Console.WriteLine(lines[i]);
            }


            // 2. 스테이지 파일 파싱(Parsing)하여 초기 데이터 구성
            // ParseStage 함수 만들기
            Player player;
            Box[] boxes;
            Wall[] walls;
            Goal[] goals;
            int pushedBox = 0;
            Game.ParseStage(lines, out player, out boxes, out walls, out goals);

            // 3. 게임 진행
            // 4. 게임이 종료되었다면 다음 스테이지를 불러오기

            

            // 잔디 정보
            Grass[] grasses =
            {
                new Grass { X = 4, Y = 1 },
                new Grass { X = 20, Y = 4 },
                new Grass { X = 20, Y = 16 },
                new Grass { X = 30, Y = 9 },
                new Grass { X = 8, Y = 7 }
            };

            Obstacle[] obstacleTable =
            {
                new Obstacle{ Weight = 50, Type = "Kill Player" },
                new Obstacle{ Weight = 50, Type = "Pass Player" }
            };
            // 박스 위치 다 섞어버리는 거 이런것도 괜찮을 듯...

            Obstacle[] obstacles =
            {
                SelectTypeOfObst(obstacleTable, 19, 20),
                SelectTypeOfObst(obstacleTable, 32, 2),
                SelectTypeOfObst(obstacleTable, 11, 13),
                SelectTypeOfObst(obstacleTable, 5, 21)
            };

            // 게임 루프 구성
            while (true)
            {
                // 입력 비동기로 받는 경우 주석 해제
                //bool checkFrameUpdate = timer.Update();
                //if(!checkFrameUpdate)
                //{
                //    continue;
                //}

                //Thread.Sleep(150);
                Console.Clear();
                Render();

                ConsoleKey key = Console.ReadKey().Key;

                // 입력 비동기로 받는 경우 주석 해제
                //ConsoleKey key = default;

                //if (Console.KeyAvailable)
                //{
                //    key = Console.ReadKey().Key;
                //}

                if (key == ConsoleKey.Spacebar)
                {
                    CheckBox();
                    continue;
                }

                Update(key);

                // 박스와 골의 처리
                int boxOnGoalCount = 0;

                // 골 지점에 박스에 존재하냐?
                for (int boxId = 0; boxId < boxes.Length; ++boxId) // 모든 골 지점에 대해서
                {
                    // 현재 박스가 골 위에 올라와 있는지 체크한다.
                    boxes[boxId].IsOnGoal = false; // 없을 가능성이 높기 때문에 false로 초기화 한다.

                    for (int goalId = 0; goalId < goals.Length; ++goalId) // 모든 박스에 대해서
                    {
                        // 박스가 골 지점 위에 있는지 확인한다.
                        if (CollisionHelper.IsCollided(boxes[boxId].X, boxes[boxId].Y, goals[goalId].X, goals[goalId].Y))
                        {
                            ++boxOnGoalCount;

                            boxes[boxId].IsOnGoal = true; // 박스가 골 위에 있다는 사실을 저장해둔다.

                            break;
                        }
                    }
                }

                // 장애물에 부딪혔을 때 어떤 장애물인지 확인하여 기능 실행
                for (int obstId = 0; obstId < obstacles.Length; ++obstId)
                {
                    if (CollisionHelper.IsCollided(player.X, player.Y, obstacles[obstId].X, obstacles[obstId].Y))
                    {
                        obstacles[obstId].RunEachFunc();
                    }
                }


                // 모든 골 지점에 박스가 올라와 있다면?
                if (boxOnGoalCount == goals.Length)
                {
                    Console.Clear();
                    Console.WriteLine("축하합니다. 클리어 하셨습니다.");

                    break;
                }


            }



            // ---------------------------------- 함수 -------------------------------


            void Update(ConsoleKey key)
            {
                MovePlayer(key, ref player);

                // 플레이어와 벽의 충돌 처리
                for (int wallId = 0; wallId < walls.Length; ++wallId)
                {
                    if (false == CollisionHelper.IsCollided(player.X, player.Y, walls[wallId].X, walls[wallId].Y))
                    {
                        continue;
                    }

                    CollisionHelper.OnCollision(() =>
                    {
                        Game.PushOut(player.MoveDirection, ref player.X, ref player.Y, in walls[wallId].X, in walls[wallId].Y);
                    });
                }


                // 박스 이동 처리
                // 플레이어가 박스를 밀었을 때라는 게 무엇을 의미하는가? => 플레이어가 이동했는데 플레이어의 위치와 박스 위치가 겹쳤다.
                for (int i = 0; i < boxes.Length; ++i)
                {
                    if (false == CollisionHelper.IsCollided(player.X, player.Y, boxes[i].X, boxes[i].Y))
                    {
                        continue;
                    }

                    Game.MoveBox(player.MoveDirection, ref boxes[i].X, ref boxes[i].Y, in player.X, in player.Y);

                    player.PushedBoxIndex = i;
                    break;
                }

                // 박스와 벽의 충돌 처리
                for (int wallId = 0; wallId < walls.Length; ++wallId)
                {
                    if (false == CollisionHelper.IsCollided(boxes[player.PushedBoxIndex].X, boxes[player.PushedBoxIndex].Y, walls[wallId].X, walls[wallId].Y))
                    {
                        continue;
                    }

                    CollisionHelper.OnCollision(() =>
                    {
                        Game.PushOut(player.MoveDirection, ref boxes[player.PushedBoxIndex].X, ref boxes[player.PushedBoxIndex].Y, walls[wallId].X, walls[wallId].Y);
                        Game.PushOut(player.MoveDirection, ref player.X, ref player.Y, boxes[player.PushedBoxIndex].X, boxes[player.PushedBoxIndex].Y);
                    });

                    break;
                }

                // 박스끼리 충돌 처리
                for (int collidedBoxId = 0; collidedBoxId < boxes.Length; ++collidedBoxId)
                {
                    // 같은 박스라면 처리할 필요가 X
                    if (player.PushedBoxIndex == collidedBoxId)
                    {
                        continue;
                    }

                    if (false == CollisionHelper.IsCollided(boxes[player.PushedBoxIndex].X, boxes[player.PushedBoxIndex].Y, boxes[collidedBoxId].X, boxes[collidedBoxId].Y))
                    {
                        continue;
                    }

                    CollisionHelper.OnCollision(() =>
                    {
                        Game.PushOut(player.MoveDirection, ref boxes[player.PushedBoxIndex].X, ref boxes[player.PushedBoxIndex].Y, boxes[collidedBoxId].X, boxes[collidedBoxId].Y);
                        Game.PushOut(player.MoveDirection, ref player.X, ref player.Y, boxes[player.PushedBoxIndex].X, boxes[player.PushedBoxIndex].Y);
                    });
                }
            }

            // 플레이어를 이동시킨다.
            void MovePlayer(ConsoleKey key, ref Player player)
            {
                if (key == ConsoleKey.LeftArrow)
                {
                    Game.MoveToLeftOfTarget(out player.X, in player.X);
                    player.MoveDirection = Direction.Left;
                }

                if (key == ConsoleKey.RightArrow)
                {
                    Game.MoveToRightOfTarget(out player.X, in player.X);
                    player.MoveDirection = Direction.Right;
                }

                if (key == ConsoleKey.UpArrow)
                {
                    Game.MoveToUpOfTarget(out player.Y, in player.Y);
                    player.MoveDirection = Direction.Up;
                }

                if (key == ConsoleKey.DownArrow)
                {
                    Game.MoveToDownOfTarget(out player.Y, in player.Y);
                    player.MoveDirection = Direction.Down;
                }
            }

            int leftShootBoxId = 0;
            int rightShootBoxId = 0;
            int upShootBoxId = 0;
            int downShootBoxId = 0;

            // 박스 shoot
            void CheckBox()
            {
                for (int boxId = 0; boxId < Game.BOX_COUNT; ++boxId)
                {
                    // 왼쪽 체크, 나중에 나머지 방향 만들기
                    if (boxes[boxId].X == player.X - 1 && boxes[boxId].Y == player.Y)
                    {
                        leftShootBoxId = boxId;
                        Thread shootingLeftThread = new Thread(() => ShootBoxToLeft(leftShootBoxId));
                        shootingLeftThread.Start();
                        continue;
                    }

                    if (boxes[boxId].X == player.X + 1 && boxes[boxId].Y == player.Y)
                    {
                        rightShootBoxId = boxId;
                        Thread shootingRightThread = new Thread(() => ShootBoxToRight(rightShootBoxId));
                        shootingRightThread.Start();
                        continue;
                    }

                    if (boxes[boxId].X == player.X && boxes[boxId].Y == player.Y - 1)
                    {
                        upShootBoxId = boxId;
                        Thread shootingUpThread = new Thread(() => ShootBoxToUp(upShootBoxId));
                        shootingUpThread.Start();
                        continue;
                    }

                    if (boxes[boxId].X == player.X && boxes[boxId].Y == player.Y + 1)
                    {
                        downShootBoxId = boxId;
                        Thread shootingDownThread = new Thread(() => ShootBoxToDown(downShootBoxId));
                        shootingDownThread.Start();
                        continue;
                    }
                }
            }

            void ShootBoxToLeft(int lShootBoxId)
            {
                // boxes[boxId]의 박스 객체가 움직이는 동작
                for (int count = 0; count < 5; ++count)
                {
                    Game.MoveToLeftOfTarget(out boxes[lShootBoxId].X, in boxes[lShootBoxId].X);
                    Thread.Sleep(100);
                }
            }

            void ShootBoxToRight(int rShootBoxId)
            {
                for (int count = 0; count < 5; ++count)
                {
                    Game.MoveToRightOfTarget(out boxes[rShootBoxId].X, in boxes[rShootBoxId].X);
                    Thread.Sleep(100);
                }
            }

            void ShootBoxToUp(int uShootBoxId)
            {
                for (int count = 0; count < 5; ++count)
                {
                    Game.MoveToUpOfTarget(out boxes[uShootBoxId].Y, in boxes[uShootBoxId].Y);
                    Thread.Sleep(100);
                }
            }

            void ShootBoxToDown(int dShootBoxId)
            {
                for (int count = 0; count < 5; ++count)
                {
                    Game.MoveToDownOfTarget(out boxes[dShootBoxId].Y, in boxes[dShootBoxId].Y);
                    Thread.Sleep(100);
                }
            }


            Obstacle SelectTypeOfObst(Obstacle[] obstacleTable, int x, int y)
            {
                Random random = new Random();
                Obstacle obstacle = new Obstacle();
                int totalWeight = GetTotalWeight(obstacleTable);

                int selectNumber = (int)(1.0 + random.NextDouble() * (totalWeight - 1) + 0.5);

                int weight = 0;
                for (int i = 0; i < obstacleTable.Length; ++i)
                {
                    weight += obstacleTable[i].Weight;
                    if (selectNumber <= weight)
                    {
                        obstacle.X = x;
                        obstacle.Y = y;
                        obstacle.Type = obstacleTable[i].Type;
                        break;
                    }
                }

                return obstacle;
            }

            int GetTotalWeight(Obstacle[] obstacleTable)
            {
                int totalWeight = 0;

                for (int i = 0; i < obstacleTable.Length; ++i)
                {
                    totalWeight += obstacleTable[i].Weight;
                }

                return totalWeight;
            }

            void Render()
            {
                // 이전 프레임을 지운다.
                Console.Clear();

                // 플레이어를 그린다.
                RenderObject(player.X, player.Y, "P", ConsoleColor.DarkYellow);

                // 골을 그린다.
                for (int i = 0; i < goals.Length; ++i)
                {
                    RenderObject(goals[i].X, goals[i].Y, "G", ConsoleColor.Red);
                }

                // 박스를 그린다.
                for (int boxId = 0; boxId < boxes.Length; ++boxId)
                {
                    string boxShape = boxes[boxId].IsOnGoal ? "O" : "B";
                    ConsoleColor color = boxes[boxId].IsOnGoal ? ConsoleColor.Magenta : ConsoleColor.White;
                    RenderObject(boxes[boxId].X, boxes[boxId].Y, boxShape, color);
                }

                // 벽을 그린다.
                for (int wallId = 0; wallId < walls.Length; ++wallId)
                {
                    RenderObject(walls[wallId].X, walls[wallId].Y, "#", ConsoleColor.Blue);
                }

                // 잔디를 그린다.
                for (int grassId = 0; grassId < grasses.Length; ++grassId)
                {
                    RenderObject(grasses[grassId].X, grasses[grassId].Y, "^^^", ConsoleColor.White);
                }

                // 장애물을 그린다.
                for (int obstId = 0; obstId < obstacles.Length; ++obstId)
                {
                    RenderObject(obstacles[obstId].X, obstacles[obstId].Y, "?", ConsoleColor.Red);
                }
            }

            // 오브젝트를 그립니다.
            void RenderObject(int x, int y, string obj, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.SetCursorPosition(x, y);
                Console.Write(obj);
            }
        }


    }
}