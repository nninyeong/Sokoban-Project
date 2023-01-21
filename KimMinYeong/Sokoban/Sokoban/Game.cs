using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LetsMakeModules
{
    public static class Game
    {
        // 기호 상수 정의
        public const int GOAL_COUNT = 3;
        public const int BOX_COUNT = GOAL_COUNT;
        public const int WALL_COUNT = 3;

        public const int MIN_X = 0;
        public const int MIN_Y = 0;
        public const int MAX_X = 40;
        public const int MAX_Y = 25;

        public static void ExitWithError(string errorMessage)
        {
            Console.Clear();
            Console.WriteLine(errorMessage);
            Environment.Exit(1);
        }

        public static void MoveToLeftOfTarget(out int x, in int target) => x = Math.Max(MIN_X, target - 1);
        public static void MoveToRightOfTarget(out int x, in int target) => x = Math.Min(target + 1, MAX_X);
        public static void MoveToUpOfTarget(out int y, in int target) => y = Math.Max(MIN_Y, target - 1);
        public static void MoveToDownOfTarget(out int y, in int target) => y = Math.Min(target + 1, MAX_Y);

        public static string[] LoadStage(int stageNumber)
        {
            // 파일을 불러온다 -> 한줄로 끊어오는 방식이 좌표 구성에 편할 것
            // 1. 경로를 구성한다.
            string stageFilePath = Path.Combine("Assets", "Stage", $"Stage{stageNumber:D2}.txt");  // Assets > Stage > Stage01.txt 같은 파일 불러옴
            Console.WriteLine(stageFilePath);
            // D2 : 포맷팅 방법 두자리를 채우는데 빈 공간은 0으로

            // Assertion (단정문?): 안전하게 프로그래밍을 하기 위한 기법, 조건문이 참인지 검사 -> 시스템이 우리 의도대로 구축되었는지 검사
            // 추후 알고리즘을 통해 함수를 만들 때 Pre-Condition(사전 조건), Post-Condition(사후 조건)을 확인함
            // 사전조건: 알고리즘 실행 전 반드시 충족되어야 할 조건
            // 사후조건: 알고리즘 실행 후 반드시 충족되어야 할 조건
            // 보통 사전조건과 사후조건을 확인하는데 Assertion을 사용함
            // 파일이 존재하는 것은 지금 하려는 파일을 불러오는 동작의 사전조건
            // if문을 통해 확인하는 것과의 차이? if문은 프로그램을 종료하진 않지만 Assertion은 false일 때 아예 종료하고 stack 어쩌고 콜..
            // 따라서 메세지를 확인하여 버그를 해결할 수 있다.
            // Debug.Asset(false); 혹은 Debug.Assert(false, "여기가 고장남"); 과 같이 함께 출력할 메세지를 입력할 수도 있음
            // 따라서 잘못된 데이터를 허용한다면 조건문을 이용할 수 있고, 아예 허용하지 않는 경우 Asset를 이용
            // 2. 파일이 존재하는지 확인
            if (!File.Exists(stageFilePath))
            {
                ExitWithError($"스테이지 파일이 없습니다. 스테이지 번호({stageNumber})");
                Environment.Exit(-1);
                // 이건 유저한테도 보이는 메세지
                // 단정문은 디버그 용도라는 관점
            }

            // 3. 파일의 내용을 불러온다.
            return File.ReadAllLines(stageFilePath);

            // Tip > stageNumber도 1 2 와같은 단순 숫자 말고 열거형으로 구성해서 Stage의 특성을 나타내는 걸로 바꿔도 됨
        }

        public static void ParseStage(string[] stage, out Player player, out Box[] boxes, out Wall[] walls, out Goal[] goals)
        {
            // 이 함수는 이미 파일을 읽어온 후(LoadStage 후) 실행하기 때문에 반드시 stage가 null이 아니어야 함
            // stage == null인 걸 허용할 수 없는 상태이므로 Asset를 사용해야 함 if말고
            Debug.Assert(stage != null);

            player = null;

            string[] stageMetadata = stage[0].Split(" ");  // 1. 첫번째 줄에서 메타데이터를 파싱한다. {벽의 개수} {박스 개수} {골 개수}
            walls = new Wall[int.Parse(stageMetadata[0])];
            boxes = new Box[int.Parse(stageMetadata[1])];
            goals = new Goal[int.Parse(stageMetadata[2])];

            int wallIndex = 0;
            int boxIndex = 0;
            int goalIndex = 0;

            for (int y = 1; y < stage.Length; ++y)
            {
                for (int x = 0; x < stage[y].Length; ++x)
                {

                    switch (stage[y][x])
                    {
                        case ObjectSymbol.Player:
                            player = new Player { X = x, Y = y - 1 };  // y좌표 -1 하는 이유는 첫줄이 메타데이터라 세로는 1부터 맵이기 때문
                            break;

                        case ObjectSymbol.Wall:
                            // 배열은 어떻게 할것인가? 가장 쉬운 방법은 최대 수를 제한하는 것, 최대 크기만큼의 배열을 선언해두고 거기에 채워주면 됨
                            // 그리고 몇개가 나왔는지 카운트할 int도 필요
                            // 혹은 파일의 첫 줄에 순서대로 어떤 것의 최대 수를 명시해둘지 적어두고 그걸 받아와서 split 하여 이용할 수도 있음
                            // 데이터는 항상 변할 수 있기 때문에 파일에 입력한 수를 가져오는게 나음
                            walls[wallIndex] = new Wall { X = x, Y = y - 1 };
                            ++wallIndex;
                            break;

                        case ObjectSymbol.Box:
                            boxes[boxIndex] = new Box { X = x, Y = y - 1 };
                            ++boxIndex;
                            break;

                        case ObjectSymbol.Goal:
                            goals[goalIndex] = new Goal { X = x, Y = y - 1 };
                            ++goalIndex;
                            break;

                        case ' ':
                            break;

                        default:
                            // 데이터는 항상 조작될 수 있기 때문에 검증할 필요가 있음
                            ExitWithError($"스테이지 파일이 잘못되었습니다.");
                            break;
                    }
                }
            }
        }

        // 충돌 처리
        public static void PushOut(Direction playerMoveDirection, ref int objX, ref int objY, in int collidedObjX, in int collidedObjY)
        {
            switch (playerMoveDirection)
            {
                case Direction.Left:
                    MoveToRightOfTarget(out objX, in collidedObjX);
                    break;

                case Direction.Right:
                    MoveToLeftOfTarget(out objX, in collidedObjX);
                    break;

                case Direction.Up:
                    MoveToDownOfTarget(out objY, in collidedObjY);
                    break;

                case Direction.Down:
                    MoveToUpOfTarget(out objY, in collidedObjY);
                    break;
            }
        }

        // 박스를 움직인다
        public static void MoveBox(Direction playerMoveDirection, ref int boxX, ref int boxY, in int playerX, in int playerY)
        {
            switch (playerMoveDirection)
            {
                case Direction.Left:
                    MoveToLeftOfTarget(out boxX, in playerX);
                    break;

                case Direction.Right:
                    MoveToRightOfTarget(out boxX, in playerX);
                    break;

                case Direction.Up:
                    MoveToUpOfTarget(out boxY, in playerY);
                    break;

                case Direction.Down:
                    MoveToDownOfTarget(out boxY, in playerY);
                    break;

                default:  // Error
                    Game.ExitWithError($"[Error] 플레이어 방향: {playerMoveDirection}");
                    break;
            }
        }
    }
}
