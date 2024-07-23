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
        public int runCommand(int _curTime, int dt)
        {
            while (getCommand() != null && getCommand().enTime <= _curTime)
            {
                dequeueCommand();
            }
            if (getCommand() == null || getCommand().stTime > _curTime)
            {
                return 0;
            }
            Command _cmd = getCommand();
            setLoadedRS(_cmd.loadedRS);
            if (_curTime + dt >= _cmd.enTime)
            {
                if (getTelemeter() != -1)
                {
                    increaseTelemeter(HaDesMath.CalcDistancePoints(getCenter(), _cmd.enPos));
                }
                setCenter(_cmd.enPos);
                setAngle(_cmd.enAngle);
                dequeueCommand();
                return _cmd.type;
            }
            else
            {
                int cmdLeft = (_cmd.enTime - _curTime) / dt;
                if (_cmd.direction == 0) // moving...
                {
                    float dx = (_cmd.enPos.X - getCenter().X) / cmdLeft;
                    float dy = (_cmd.enPos.Y - getCenter().Y) / cmdLeft;
                    setAngle(_cmd.enAngle);
                    move(dx, dy);
                }
                else // rotating...
                {
                    float _ang = Math.Abs(_cmd.enAngle - getAngle());
                    rotate((float)_cmd.direction * _ang / cmdLeft);
                    setCenter(_cmd.enPos);
                }
            }
            return 0;
        }
        public void run(int _elapsed)
        {
            //processPassingLCCs(180);
            for (int i = 0; i < Global.LCCCounts; i++)
            {
                switch (Global._lcc[i].getStatus())
                {
                    case 1:  // waiting to check shunt(before carry)
                        if (checkStorage())
                        {
                            int routeNum = Global.curStorage == 0 ? 11 : 21;
                            if (checkRoute(routeNum) && checkRoute(routeNum + 1) && checkRoute(routeNum + 2))
                            {
                                if (Global.curStorage == 0)
                                {
                                    gotoShuntInA(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                                }
                                else
                                {
                                    gotoShuntInB(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                                }
                                if (Global.waitingRoom.Count > 0)
                                {
                                    Global.waitingRoom.RemoveAt(0);
                                }
                                Global._lcc[i].setStatus(routeNum);
                                Global.stockedRSinStorage++;
                            }
                        }
                        break;
                    case 2:  // wait to load new RS (after carry)
                        Global.waitingRoom.Add(i);
                        PointF curPos = PointF.Empty;
                        float tempAng = -90;
                        if (Global.routeDozerPoints.Count > 0)
                        {
                            curPos = Global.routeDozerPoints[Global.routeDozerPoints.Count - 1];
                        }
                        curPos.Y += 5 * Global.waitingRoom.Count;
                        Global._lcc[i].setCenter(curPos);
                        Global._lcc[i].setAngle(tempAng);
                        Global._lcc[i].clearCommand();
                        Global._lcc[i].setStatus(0);
                        break;
                    case 12:  // check on shunt on A 
                        if (checkRoute(14))
                        {
                            gotoLocAandBackToShunt(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), false);
                            Global._lcc[i].setStatus(14);
                        }
                        else
                        {
                            waitShuntInA(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                            Global._lcc[i].setStatus(11);
                        }
                        break;
                    case 13:  // wait on shunt in A to prev pass
                        if (checkRoute(14))
                        {
                            gotoLocAandBackToShunt(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), true);
                            Global._lcc[i].setStatus(14);
                        }
                        break;
                    case 15:  // check on shunt on A back
                        if (checkRoute(11))
                        {
                            gotoDozerInA(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), false);
                            Global._lcc[i].setStatus(11);
                        }
                        else
                        {
                            waitShuntInA(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                            Global._lcc[i].setStatus(14);
                        }
                        break;
                    case 16:  // check on shunt on A back
                        if (checkRoute(11))
                        {
                            gotoDozerInA(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), true);
                            Global._lcc[i].setStatus(11);
                        }
                        break;
                    case 22:  // check on shunt on B 
                        if (checkRoute(24))
                        {
                            gotoLocBandBackToShunt(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), false);
                            Global._lcc[i].setStatus(24);
                        }
                        else
                        {
                            waitShuntInB(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                            Global._lcc[i].setStatus(21);
                        }
                        break;
                    case 23:  // wait on shunt in B to prev pass
                        if (checkRoute(24))
                        {
                            gotoLocBandBackToShunt(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), true);
                            Global._lcc[i].setStatus(24);
                        }
                        break;
                    case 25:  // check on shunt on B back
                        if (checkRoute(21))
                        {
                            gotoDozerInB(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), false);
                            Global._lcc[i].setStatus(21);
                        }
                        else
                        {
                            waitShuntInB(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS());
                            Global._lcc[i].setStatus(24);
                        }
                        break;
                    case 26:  // check on shunt on B back
                        if (checkRoute(21))
                        {
                            gotoDozerInB(i, Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), _elapsed, Global._lcc[i].getLoadedRS(), true);
                            Global._lcc[i].setStatus(21);
                        }
                        break;
                    default:
                        break;
                }

                int status = Global._lcc[i].runCommand(_elapsed, Global.SimulationSpeed * Global.timerInterval / 1000);
                if (Global._lcc[i].getLoadedRS() != -1)
                {
                    int id = Global._lcc[i].getLoadedRS();
                    Global._rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(Global._lcc[i].getCenter(), Global._lcc[i].getAngle(), 2.5f));
                    Global._rsData[id].setAngle(Global._lcc[i].getAngle());
                }
                if (status == 1 || status == 2)
                {
                    Global._lcc[i].setStatus(status);
                }
                else if (status == 4)
                {
                    Global.rsInLocA.Add(Global._lcc[i].getLoadedRS());
                    Global.lastRSPosInA = HaDesMath.movePointByDisAndAngle(Global.lastRSPosInA, HaDesMath.getAngleByPoints(Global.locAPosEn, Global.locAPosSt), 5.0f);
                }
                else if (status == 5)
                {
                    Global.rsInLocB.Add(Global._lcc[i].getLoadedRS());
                    Global.lastRSPosInB = HaDesMath.movePointByDisAndAngle(Global.lastRSPosInB, HaDesMath.getAngleByPoints(Global.locBPosEn, Global.locBPosSt), 5.0f);
                }
                else if (status > 10)
                {
                    Global._lcc[i].setStatus(status);
                }
            }
        }

        // check storages are full or not t: 0, A, t: 1, B
        private bool checkStorage()
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

        private bool checkRoute(int segNum)
        {
            for (int i = 0; i < Global.LCCCounts; i++)
            {
                if (Global._lcc[i].getStatus() == segNum) return false;
            }
            return true;
        }

        private void gotoShuntInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeAPoints, out m, out interPos);

            // move in route A: carry
            for (int i = 1; i <= m; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, idRS, 12);
            Global._lcc[id].addCommand(_cmd);
        }
        private void waitShuntInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAng = lastAng;
            Command _cmd;
            int type = Global._lcc[id].getStatus() + 1;
            _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng - 90, 8.0f), curAng + 90, curAng + 90, 0, idRS, type);
            Global._lcc[id].addCommand(_cmd);
        }
        private void gotoLocAandBackToShunt(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeAPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m + 1; i < Global.routeAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, Global.lastRSPosInA);
            alpha = HaDesMath.getAngleByPoints(Global.lastRSPosInA, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, Global.lastRSPosInA, alpha, alpha, 0, idRS);
            Global._lcc[id].addCommand(_cmd);
            curPos = Global.lastRSPosInA;
            curAng = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 60, curPos, curPos, curAng, curAng + 180, 1, idRS, 4);
            Global._lcc[id].addCommand(_cmd);
            curAng = curAng + 180;
            curTime += Global.LCCDropTime * 60;
            // move in route A: back
            for (int i = Global.routeAPoints.Count - 1; i > m; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, -1);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, -1, 15);
            Global._lcc[id].addCommand(_cmd);
        }
        private void gotoDozerInA(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeAPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, -1, type);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curAng = alpha;
                curTime += step;
            }
        }

        private void gotoShuntInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeBPoints, out m, out interPos);

            // move in route A: carry
            for (int i = 1; i <= m; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, idRS, 22);
            Global._lcc[id].addCommand(_cmd);
        }
        private void waitShuntInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAng = lastAng;
            Command _cmd;
            int type = Global._lcc[id].getStatus() + 1;
            _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng - 90, 8.0f), curAng + 90, curAng + 90, 0, idRS, type);
            Global._lcc[id].addCommand(_cmd);
        }
        private void gotoLocBandBackToShunt(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeBPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m + 1; i < Global.routeBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, Global.lastRSPosInB);
            alpha = HaDesMath.getAngleByPoints(Global.lastRSPosInB, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, Global.lastRSPosInB, alpha, alpha, 0, idRS);
            Global._lcc[id].addCommand(_cmd);
            curPos = Global.lastRSPosInB;
            curAng = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 60, curPos, curPos, curAng, curAng + 180, 1, idRS, 5);
            Global._lcc[id].addCommand(_cmd);
            curAng = curAng + 180;
            curTime += Global.LCCDropTime * 60;
            // move in route A: back
            for (int i = Global.routeBPoints.Count - 1; i > m; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, -1);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
            d = HaDesMath.CalcDistancePoints(curPos, interPos);
            alpha = HaDesMath.getAngleByPoints(interPos, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, interPos, alpha, alpha, 0, -1, 25);
            Global._lcc[id].addCommand(_cmd);
        }
        private void gotoDozerInB(int id, PointF lastPos, float lastAng, int lastTime, int idRS, bool isWaiting)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float alpha, d, curAng = lastAng;
            Command _cmd;
            int m;
            PointF interPos;
            float shunt = HaDesMath.calcShuntDisAndPos(Global.locShuntPos, Global.routeBPoints, out m, out interPos);

            if (isWaiting)
            {
                _cmd = new Command(curTime, curTime + Global.LCCWaitingTime * 60, curPos, HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f), curAng, curAng, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = HaDesMath.movePointByDisAndAngle(curPos, curAng, 8.0f);
                curTime += Global.LCCWaitingTime * 60;
            }

            // move in route A: carry
            for (int i = m; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, -1, type);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curAng = alpha;
                curTime += step;
            }
        }




































        //////////////////////////// Unusing for now ////////////////////////////////////
        ///

        /*
        private void insertWaitingCommand(int waitingTime, int id)
        {
            Queue<Command> _cmdTemp = new Queue<Command>();

            foreach (Command cmd in _cmdLCC[id])
            {
                _cmdTemp.Enqueue(cmd);
            }
            _cmdLCC[id].Clear();
            Command _cmd = new Command(_elapsed, _elapsed + waitingTime, Global._lcc[id].getCenter(), Global._lcc[id].getCenter(), Global._lcc[id].getAngle(), Global._lcc[id].getAngle(), 0, _cmdTemp.Peek().loadedRS, 3);
            _cmdLCC[id].Enqueue(_cmd);

            _cmd = _cmdTemp.Dequeue();
            _cmd.stAngle = Global._lcc[id].getAngle();
            _cmd.stPos = Global._lcc[id].getCenter();
            _cmd.stTime = _elapsed + waitingTime;
            if (_cmd.enTime != int.MaxValue) _cmd.enTime = _cmd.enTime + waitingTime;
            _cmdLCC[id].Enqueue(_cmd);

            while (_cmdTemp.Count > 0)
            {
                _cmd = _cmdTemp.Dequeue();
                _cmd.stTime += waitingTime;
                if (_cmd.enTime != int.MaxValue) _cmd.enTime = _cmd.enTime + waitingTime;
                _cmdLCC[id].Enqueue(_cmd);
            }
        }

        private void processPassingLCCs(int waitingTime)
        {
            float dl = (float)Global.LCCSpeed * Global.SimulationSpeed * timer1.Interval / 3600.0f;
            for (int i = 0; i < Global.LCCCounts; i ++)
            {
                if (Global._lcc[i].getTelemeter() <= 0 || _cmdLCC[i].Count == 0) continue;
                if (HaDesMath.CalcDistancePoints(Global._lcc[i].getCenter(), Global.routeAPoints[0]) < 3.0f * dl) continue;
                if (_cmdLCC[i].Peek().type == 3) // waiting...
                {
                    continue;
                }
                for (int j = i + 1; j < Global.LCCCounts; j ++)
                {
                    if (Global._lcc[j].getTelemeter() <= 0 || _cmdLCC[j].Count == 0) continue;
                    if (HaDesMath.CalcDistancePoints(Global._lcc[j].getCenter(), Global.routeAPoints[0]) < 3.0f * dl) continue;
                    if (_cmdLCC[j].Peek().type == 3) // waiting...
                    {
                        continue;
                    }
                    if (HaDesMath.CalcDistancePoints(Global._lcc[i].getCenter(), Global._lcc[j].getCenter()) < dl * 1.05f)
                    {
                        insertWaitingCommand(waitingTime, i);
                        insertWaitingCommand(waitingTime, j);
                    }
                }
            }
        } */

        private void carryRSToLocA(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in route A: carry
            for (int i = 1; i < Global.routeAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            // go to location A load pos
            d = HaDesMath.CalcDistancePoints(curPos, Global.lastRSPosInA);
            alpha = HaDesMath.getAngleByPoints(Global.lastRSPosInA, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, Global.lastRSPosInA, alpha, alpha, 0, idRS);
            Global._lcc[id].addCommand(_cmd);
            curPos = Global.lastRSPosInA;
            curAngle = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, idRS, 4);
            Global._lcc[id].addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += Global.LCCDropTime * 60;


            // move in route A: back
            for (int i = Global.routeAPoints.Count - 1; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeAPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeAPoints[i], alpha, alpha, 0, -1, type);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeAPoints[i];
                curAngle = alpha;
                curTime += step;
            }
        }


        private void carryRSToLocB(int id, PointF lastPos, float lastAng, int lastTime, int idRS)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;


            // move in route B: carry
            for (int i = 1; i < Global.routeBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, idRS);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            // go to location B load pos
            d = HaDesMath.CalcDistancePoints(curPos, Global.lastRSPosInB);
            alpha = HaDesMath.getAngleByPoints(Global.lastRSPosInB, curPos);

            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, Global.lastRSPosInB, alpha, alpha, 0, idRS);
            Global._lcc[id].addCommand(_cmd);
            curPos = Global.lastRSPosInB;
            curAngle = alpha;
            curTime += step;

            // drop RS
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, idRS, 5);
            Global._lcc[id].addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += Global.LCCDropTime * 60;


            // move in route B: back
            for (int i = Global.routeBPoints.Count - 1; i >= 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeBPoints[i], curPos);
                int type = (i == 0) ? 2 : 0;
                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeBPoints[i], alpha, alpha, 0, -1, type);
                Global._lcc[id].addCommand(_cmd);
                curPos = Global.routeBPoints[i];
                curAngle = alpha;
                curTime += step;
            }
        }
    }
}
