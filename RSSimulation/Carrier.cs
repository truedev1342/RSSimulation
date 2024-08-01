using System;
using System.Collections.Generic;
using System.Drawing;

namespace RSSimulation
{
    class Carrier : Vehicle
    {
        protected float telemeter { get; set; }
        protected int id { get; set; }

        public Carrier() : base()
        {
            telemeter = 0.0f;
            id = 0;
            imgV = Properties.Resources.LCC;
        }
        public Carrier(int _id) : base()
        {
            telemeter = 0.0f;
            id = _id;
            imgV = Properties.Resources.LCC;
        }
        public float getTelemeter() { return telemeter; }
        public void setTelemeter(float _telemeter) { telemeter = _telemeter; }
        public int getId() { return id; }
        public void setId(int _id) { id = _id; }
        public void increaseTelemeter(float d) { telemeter += d; }

        protected void moveRS()
        {
            int id = loadedRS;
            if (id == -1) return;
            Global._rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(center, angle, 2.5f));
            Global._rsData[id].setAngle(angle);
        }

        public void run(int _elapsed)
        {
            while (true)
            {
                processBreaking(_elapsed);

                int lastTime = runCommand(_elapsed);
                moveRS();
                if (status < 100 && status > 0)
                {
                    processBreaking(lastTime);
                    dequeueCommand();
                    runCommand(_elapsed);
                }
                if (getCommand() == null || getCommand().enTime > _elapsed)
                {
                    break;
                }
                dequeueCommand();
            }
        }

        protected void processBreaking(int _curTime)
        {
            switch (status)
            {
                case Constants.cmdWaiting:  // waiting to check shunt(before carry)
                    if (checkStorage())
                    {
                        int routeNum = Global.curStorage == 0 ? Constants.cmdMovingOnAPart1 : Constants.cmdMovingOnBPart1;
                        int routeNum1 = Global.curStorage == 0 ? Constants.cmdCheckOnShuntA : Constants.cmdCheckOnShuntB;
                        int routeNum2 = Global.curStorage == 0 ? Constants.cmdCheckOnShuntAWaiting : Constants.cmdCheckOnShuntBWaiting;
                        int routeNum3 = Global.curStorage == 0 ? Constants.cmdMovingOnAPart4 : Constants.cmdMovingOnBPart4;
                        if (checkRoute(routeNum) && checkRoute(routeNum1) && checkRoute(routeNum2) && checkRoute(routeNum3))
                        {
                            carryToShunt(Global.curStorage, center, angle, _curTime);
                            if (Global.waitingRoom.Count > 0)
                            {
                                Global.waitingRoom.RemoveAt(0);
                            }
                            setStatus(routeNum);
                            Global.stockedRSinStorage++;
                        }
                    }
                    break;
                case Constants.cmdCarried:  // wait to load new RS (after carry)
                    PointF curPos = PointF.Empty;
                    float tempAng = -90;
                    if (Global.waitingRoom.Count > 0)
                    {
                        curPos = Global._lcc[Global.waitingRoom[Global.waitingRoom.Count - 1]].center;
                        curPos.Y += 5.0f;
                    } else
                    {
                        if (Global.routeDozerPoints.Count > 0)
                        {
                            curPos = Global.routeDozerPoints[Global.routeDozerPoints.Count - 1];
                        }
                        curPos.Y += 5.0f;
                    }
                    setCenter(curPos);
                    setAngle(tempAng);
                    clearCommand();
                    setStatus(0);
                    Global.waitingRoom.Add(id);
                    break;
                case Constants.cmdDrapLocA:
                    Global.rsInLocA.Add(loadedRS);
                    Global.lastRSPosInA = HaDesMath.movePointByDisAndAngle(Global.lastRSPosInA, HaDesMath.getAngleByPoints(Global.locAPosEn, Global.locAPosSt), 5.0f);
                    break;
                case Constants.cmdDrapLocB:
                    Global.rsInLocB.Add(loadedRS);
                    Global.lastRSPosInB = HaDesMath.movePointByDisAndAngle(Global.lastRSPosInB, HaDesMath.getAngleByPoints(Global.locBPosEn, Global.locBPosSt), 5.0f);
                    break;
                case Constants.cmdCheckOnShuntA:  // check on shunt on A 
                    if (checkRoute(Constants.cmdMovingOnAPart2) && checkRoute(Constants.cmdMovingOnAPart3) && checkRoute(Constants.cmdCheckOnLocA))
                    {
                        carryToStorage(0, center, angle, _curTime, false);
                        setStatus(Constants.cmdMovingOnAPart2);
                    }
                    else
                    {
                        waitInShunt(0, center, angle, _curTime);
                        setStatus(Constants.cmdMovingOnAPart1);
                    }
                    break;
                case Constants.cmdCheckOnShuntAWaiting:  // wait on shunt in A to prev pass
                    if (checkRoute(Constants.cmdMovingOnAPart2) && checkRoute(Constants.cmdMovingOnAPart3) && checkRoute(Constants.cmdCheckOnLocA))
                    {
                        carryToStorage(0, center, angle, _curTime, true);
                        setStatus(Constants.cmdMovingOnAPart2);
                    }
                    break;
                case Constants.cmdCheckOnLocA:  // check on loc A start Point
                    /*int cSA = Global.checkRSinStorage(Global.rsInLocA);
                    if (cSA == -1)
                    {
                        Global.rsInLocA.Clear();
                        Global.lastRSPosInA = HaDesMath.movePointByDisAndAngle(Global.locAPosEn, HaDesMath.getAngleByPoints(Global.locAPosEn, Global.locAPosSt), 5.0f);
                    }*/
                    backToShunt(0, center, angle, _curTime);
                    setStatus(Constants.cmdMovingOnAPart2);
                    break;
                case Constants.cmdCheckOnShuntABack:  // check on shunt on A back
                    if (checkRoute(Constants.cmdMovingOnAPart1))
                    {
                        backToDozer(0, center, angle, _curTime, false);
                        setStatus(Constants.cmdMovingOnAPart4);
                    }
                    else
                    {
                        waitInShunt(1, center, angle, _curTime);
                        setStatus(Constants.cmdMovingOnAPart3);
                    }
                    break;
                case Constants.cmdCheckOnShuntAWaitingBack:  // check on shunt on A back
                    if (checkRoute(Constants.cmdMovingOnAPart1))
                    {
                        backToDozer(0, center, angle, _curTime, true);
                        setStatus(Constants.cmdMovingOnAPart4);
                    }
                    break;
                case Constants.cmdCheckOnShuntB:  // check on shunt on B 
                    if (checkRoute(Constants.cmdMovingOnBPart2) && checkRoute(Constants.cmdMovingOnBPart3) && checkRoute(Constants.cmdCheckOnLocB))
                    {
                        carryToStorage(1, center, angle, _curTime, false);
                        setStatus(Constants.cmdMovingOnBPart2);
                    }
                    else
                    {
                        waitInShunt(2, center, angle, _curTime);
                        setStatus(Constants.cmdMovingOnBPart1);
                    }
                    break;
                case Constants.cmdCheckOnShuntBWaiting:  // wait on shunt in B to prev pass
                    if (checkRoute(Constants.cmdMovingOnBPart2) && checkRoute(Constants.cmdMovingOnBPart3) && checkRoute(Constants.cmdCheckOnLocB))
                    {
                        carryToStorage(1, center, angle, _curTime, true);
                        setStatus(Constants.cmdMovingOnBPart2);
                    }
                    break;
                case Constants.cmdCheckOnLocB:  // check on loc B start Point
                    /*int cSB = Global.checkRSinStorage(Global.rsInLocB);
                    if (cSB == -1)
                    {
                        Global.rsInLocB.Clear();
                        Global.lastRSPosInB = HaDesMath.movePointByDisAndAngle(Global.locBPosEn, HaDesMath.getAngleByPoints(Global.locBPosEn, Global.locBPosSt), 5.0f);
                    }*/
                    backToShunt(1, center, angle, _curTime);
                    setStatus(Constants.cmdMovingOnBPart2);
                    break;
                case Constants.cmdCheckOnShuntBBack:  // check on shunt on B back
                    if (checkRoute(Constants.cmdMovingOnBPart1))
                    {
                        backToDozer(1, center, angle, _curTime, false);
                        setStatus(Constants.cmdMovingOnBPart4);
                    }
                    else
                    {
                        waitInShunt(3, center, angle, _curTime);
                        setStatus(Constants.cmdMovingOnBPart3);
                    }
                    break;
                case Constants.cmdCheckOnShuntBWaitingBack:  // check on shunt on B back
                    if (checkRoute(Constants.cmdMovingOnBPart1))
                    {
                        backToDozer(1, center, angle, _curTime, true);
                        setStatus(Constants.cmdMovingOnBPart4);
                    }
                    break;
                default:
                    break;
            }
        }

        // check storages are full or not t: 0, A, t: 1, B
        protected bool checkStorage()
        {
            int maxS;
            if (Global.curStorage == 0)
            {
                maxS = (int)(HaDesMath.CalcDistancePoints(Global.locAPosSt, Global.locAPosEn) / 5.0f);
                if (Global.stockedRSinStorage < maxS) return true;
                if (Global.checkRSinStorage(Global.rsInLocB) == -1)
                {
                    Global.rsInLocB.Clear();
                    Global.lastRSPosInB = HaDesMath.movePointByDisAndAngle(Global.locBPosEn, HaDesMath.getAngleByPoints(Global.locBPosEn, Global.locBPosSt), 5.0f);
                    Global.curStorage = 1;
                    Global.stockedRSinStorage = 0;
                    return true;
                }
                Global.curStorage = 1;
                return false;
            }
            maxS = (int)(HaDesMath.CalcDistancePoints(Global.locBPosSt, Global.locBPosEn) / 5.0f);
            if (Global.stockedRSinStorage < maxS) return true;
            if (Global.checkRSinStorage(Global.rsInLocA) == -1)
            {
                Global.rsInLocA.Clear();
                Global.lastRSPosInA = HaDesMath.movePointByDisAndAngle(Global.locAPosEn, HaDesMath.getAngleByPoints(Global.locAPosEn, Global.locAPosSt), 5.0f);
                Global.stockedRSinStorage = 0;
                Global.curStorage = 0;
                return true;
            }
            Global.curStorage = 0;
            return false;
        }

        protected bool checkRoute(int segNum)
        {
            for (int i = 0; i < Global.LCCCounts; i++)
            {
                if (Global._lcc[i].status == segNum) return false;
            }
            return true;
        }


        protected void carryToShunt(int route, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d;
            Command _cmd;
            int m;
            List<PointF> routePoints = route == 0 ? Global.routeAPoints : Global.routeBPoints;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, routePoints, out m, out interPos);

            // move in route A: carry
            for (int i = 1; i <= m; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routePoints[i]);
                alpha = HaDesMath.getAngleByPoints(routePoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routePoints[i], alpha, alpha, 0, loadedRS, Constants.cmdMovingOnAPart1 + route * 10);
                addCommand(_cmd);
                curPos = routePoints[i];
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, loadedRS, Constants.cmdCheckOnShuntA + route * 10);
            addCommand(_cmd);
        }

        protected void waitInShunt(int route, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAng = lastAng;
            Command _cmd;
            int type = Constants.cmdCheckOnShuntAWaiting; ;
            if (route == 0)
            {
                type = Constants.cmdCheckOnShuntAWaiting;
            } else if (route == 1)
            {
                type = Constants.cmdCheckOnShuntAWaitingBack;
            } else if (route == 2)
            {
                type = Constants.cmdCheckOnShuntBWaiting;
            }
            else if (route == 3)
            {
                type = Constants.cmdCheckOnShuntBWaitingBack;
            }
            _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng - 90, 8.0f), curAng + 90, curAng + 90, 0, loadedRS, type);
            addCommand(_cmd);
        }

        protected void carryToStorage(int route, PointF lastPos, float lastAng, int lastTime, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            List<PointF> routePoints = route == 0 ? Global.routeAPoints : Global.routeBPoints;
            PointF lastRSPos = route == 0 ? Global.lastRSPosInA : Global.lastRSPosInB;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, routePoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, loadedRS, Constants.cmdMovingOnAPart2 + route * 10);
                addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m + 1; i < routePoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routePoints[i]);
                alpha = HaDesMath.getAngleByPoints(routePoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                int type = i == routePoints.Count - 1 ? Constants.cmdCheckOnLocA + route * 10 : Constants.cmdMovingOnAPart2 + route * 10;
                _cmd = new Command(curTime, curTime + step, curPos, routePoints[i], alpha, alpha, 0, loadedRS, type);
                addCommand(_cmd);
                curPos = routePoints[i];
                curAng = alpha;
                curTime += step;
            }
        }

        protected void backToShunt(int route, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            List<PointF> routePoints = route == 0 ? Global.routeAPoints : Global.routeBPoints;
            PointF lastRSPos = route == 0 ? Global.lastRSPosInA : Global.lastRSPosInB;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, routePoints, out m, out interPos);

            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, lastRSPos);
            alpha = HaDesMath.getAngleByPoints(lastRSPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, lastRSPos, alpha, alpha, 0, loadedRS, Constants.cmdMovingOnAPart2 + route * 10);
            addCommand(_cmd);
            curPos = lastRSPos;
            curAng = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 60, curPos, curPos, curAng, curAng + 180, 1, loadedRS, Constants.cmdDrapLocA + route);
            addCommand(_cmd);
            curAng = curAng + 180;
            curTime += Global.LCCDropTime * 60;
            // move in route A: back
            for (int i = routePoints.Count - 1; i > m; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routePoints[i]);
                alpha = HaDesMath.getAngleByPoints(routePoints[i], curPos);
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routePoints[i], alpha, alpha, 0, -1, Constants.cmdMovingOnAPart3 + route * 10);
                addCommand(_cmd);
                curPos = routePoints[i];
                curAng = alpha;
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, -1, Constants.cmdCheckOnShuntABack + route * 10);
            addCommand(_cmd);
        }

        protected void backToDozer(int route, PointF lastPos, float lastAng, int lastTime, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            List<PointF> routePoints = route == 0 ? Global.routeAPoints : Global.routeBPoints;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, routePoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, loadedRS, Constants.cmdMovingOnAPart4 + route * 10);
                addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routePoints[i]);
                alpha = HaDesMath.getAngleByPoints(routePoints[i], curPos);
                int type = (i == 0) ? Constants.cmdCarried : Constants.cmdMovingOnAPart4 + route * 10;
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routePoints[i], alpha, alpha, 0, -1, type);
                addCommand(_cmd);
                curPos = routePoints[i];
                curAng = alpha;
                curTime += step;
            }
        }
    }
}
