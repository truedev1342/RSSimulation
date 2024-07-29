using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RSSimulation
{
    class Global
    {
        public static DateTime SimulationTime = DateTime.Now;
        public static int SimulationSpeed = 3600;
        public static int RSCounts = 149;
        public static int EFrameBuiltTime = 48;   //unit : hour
        public static int SalvageTime = 60;       //unit : min
        public static float DozerSpeed = 0.12f;        //unit : km/h
        public static float LCCSpeed = 0.06f;          //unit : km/h
        public static int LCCLoadingTime = 60;    //unit : min
        public static int LCCDropTime = 60;       //unit : min
        public static int LCCCounts = 3;
        public static int LCCWaitingTime = 3;     //unit : min

        public static RoofSupport[] _rsData = new RoofSupport[202];
        public static Dozer _dozer = new Dozer();
        public static Carrier[] _lcc = new Carrier[3];
        public static SurfaceCarrier _surfLcc = new SurfaceCarrier();

        public static LinePos initRSPos = new LinePos();
        public static PointF eFramePoint = PointF.Empty;
        public static PointF _basePoint = PointF.Empty;
        public static List<int> _normalRSList = new List<int>();
        public static List<int> _salvageRSList = new List<int>();

        public static PointF locAPosSt = PointF.Empty;
        public static PointF locAPosEn = PointF.Empty;

        public static PointF locBPosSt = PointF.Empty;
        public static PointF locBPosEn = PointF.Empty;

        public static PointF locShuntPos = PointF.Empty;
        public static PointF locSurfacePos = PointF.Empty;

        public static List<PointF> routeAPoints = new List<PointF>();
        public static List<PointF> routeBPoints = new List<PointF>();
        public static List<PointF> routeDozerPoints = new List<PointF>();
        public static List<PointF> routeSurfaceAPoints = new List<PointF>();
        public static List<PointF> routeSurfaceBPoints = new List<PointF>();

        public static PointF routeAcurPos = PointF.Empty;
        public static PointF routeBcurPos = PointF.Empty;
        public static PointF routeDozercurPos = PointF.Empty;
        public static PointF routeSurfaceAcurPos = PointF.Empty;
        public static PointF routeSurfaceBcurPos = PointF.Empty;

        public static PointF lastRSPosInA = PointF.Empty;
        public static PointF lastRSPosInB = PointF.Empty;

        public static int movedSalvageRS = 0;
        public static List<int> rsInLocA = new List<int>();
        public static List<int> rsInLocB = new List<int>();
        public static List<int> rsInSurface = new List<int>();
        public static int curStorage = 0; // 0: carry to storage A, 1: carry to storage B
        public static int stockedRSinStorage = 0; // number of RSs that carried to storage
        public static int curStorageInSurface = 0;  // 0: carry from storage A, 1: carry from storage B

        public static List<int> waitingRoom = new List<int>();

        public static int timerInterval = 100;

        public static bool simulationCompleted = false;
        public static void InitializeLCCs()
        {
            PointF curPos = PointF.Empty;
            float tempAng = -90;
            if (routeDozerPoints.Count > 0)
            {
                curPos = routeDozerPoints[routeDozerPoints.Count - 1];
            }
            waitingRoom.Clear();
            for (int i = 0; i < Global.LCCCounts; i++)
            {
                curPos.Y += 5;
                _lcc[i].setCenter(curPos);
                _lcc[i].setAngle(tempAng);
                _lcc[i].clearCommand();
                _lcc[i].setStatus(0);
                _lcc[i].setLoadedRS(-1);
                waitingRoom.Add(i);
            }
            rsInLocA.Clear();
            rsInLocB.Clear();
            lastRSPosInA = HaDesMath.movePointByDisAndAngle(locAPosEn, HaDesMath.getAngleByPoints(locAPosEn, locAPosSt), 5.0f);
            lastRSPosInB = HaDesMath.movePointByDisAndAngle(locBPosEn, HaDesMath.getAngleByPoints(locBPosEn, locBPosSt), 5.0f);

            _surfLcc.setCenter(locSurfacePos);
            _surfLcc.setAngle(90);
            _surfLcc.setTelemeter(0);
            _surfLcc.setStatus(Constants.cmdSurfaceWaiting);
            _surfLcc.setLoadedRS(-1);
            _surfLcc.clearCommand();
            rsInSurface.Clear();

            curStorage = 0;
            stockedRSinStorage = 0;
            curStorageInSurface = 0;
        }

        public static void InitializeRSandDozer()
        {
            if (eFramePoint == PointF.Empty)
            {
                return;
            }
            _basePoint = HaDesMath.CalcBasePoint(initRSPos, eFramePoint);
            float alpha = HaDesMath.getAngleByPoints(initRSPos.StartPos, initRSPos.EndPos);

            int idx = Global.RSCounts - getNextMoveId(Global.RSCounts + 1);

            PointF pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 2.5f);
            pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + 2.0f * idx);
            _dozer.setAngle(alpha - 90);
            _dozer.setCenter(pos);
            _dozer.setStatus(0);
            _dozer.setLoadedRS(-1);
            _dozer.clearCommand();

            for (int i = 0; i < Global.RSCounts; i++)
            {
                pos = HaDesMath.movePointByDisAndAngle(_basePoint, alpha - 90, 8.5f);
                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 2.0f + 2.0f * i);

                _rsData[Global.RSCounts - i].setNum(Global.RSCounts - i);
                _rsData[Global.RSCounts - i].setCenter(pos);
                _rsData[Global.RSCounts - i].setAngle(alpha - 90);
            }
        }

        public static int getNextMoveId(int id)
        {
            for (int i = id - 1; i >= 0; i--)
            {
                if (!isSalvage(i)) return i;
            }
            return 0;
        }

        public static bool isSalvage(int id)
        {
            for (int i = 0; i < _salvageRSList.Count; i++)
            {
                if (_salvageRSList[i] == id)
                {
                    return true;
                }
            }
            return false;
        }

        public static int checkRSinStorage(List<int> rss)
        {
            for (int i = 0; i < rss.Count; i++)
            {
                if (rss[i] != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void run(int _elapsed)
        {
            _dozer.run(_elapsed);
            for (int i = 0; i < LCCCounts; i++)
            {
                _lcc[i].run(_elapsed);
            }
            _surfLcc.run(_elapsed);
        }
    }

    public class Command
    {
        public int stTime; // command start time.
        public int enTime; // command end time.
        public PointF stPos;  // start position when move
        public PointF enPos;  // end position when move
        public float stAngle; // start angle when rotate
        public float enAngle; // end angle when rotate
        public int direction; // move direction 1: rotate clockwise, 0: move forword, -1: rotate anti-clockwise
        public int loadedRS;  // roaded RS numb -1: not loaded
        public int type;   // command type:  used to determine next command

        public Command()
        {
            stTime = 0;
            enTime = 0;
            stPos = PointF.Empty;
            enPos = PointF.Empty;
            stAngle = 0;
            enAngle = 0;
            direction = -1;
            loadedRS = -1;
            type = 0;
        }
        public Command(int _stTime, int _enTime, PointF _stPos, PointF _enPos, float _stAngle, float _enAngle, int _direction = 0, int _loadedRS = -1, int _type = 0)
        {
            stTime = _stTime;
            enTime = _enTime;
            stPos = _stPos;
            enPos = _enPos;
            stAngle = _stAngle;
            enAngle = _enAngle;
            direction = _direction;
            loadedRS = _loadedRS;
            type = _type;
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }

    public class Constants
    {
        public const int cmdWaiting = 1;
        public const int cmdCarried = 2;
        public const int cmdSurfaceWaiting = 3;
        public const int cmdDrapLocA = 4;
        public const int cmdDrapLocB = 5;
        public const int cmdLoadFromlocA = 6;
        public const int cmdLoadFromlocB = 7;
        public const int cmdCheckOnShuntA = 11;
        public const int cmdCheckOnShuntAWaiting = 12;
        public const int cmdCheckOnLocA = 13;
        public const int cmdCheckOnShuntABack = 14;
        public const int cmdCheckOnShuntAWaitingBack = 15;
        public const int cmdCheckOnShuntB = 21;
        public const int cmdCheckOnShuntBWaiting = 22;
        public const int cmdCheckOnLocB = 23;
        public const int cmdCheckOnShuntBBack = 24;
        public const int cmdCheckOnShuntBWaitingBack = 25;
        public const int cmdMovingOnAPart1 = 116;
        public const int cmdMovingOnAPart2 = 117;
        public const int cmdMovingOnAPart3 = 118;
        public const int cmdMovingOnAPart4 = 119;
        public const int cmdMovingOnBPart1 = 126;
        public const int cmdMovingOnBPart2 = 127;
        public const int cmdMovingOnBPart3 = 128;
        public const int cmdMovingOnBPart4 = 129;
    }
}
