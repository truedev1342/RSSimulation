using System;
using System.Collections.Generic;
using System.Drawing;


namespace RSSimulation
{
    class Dozer : Vehicle
    {
        public Dozer() : base()
        {
            imgV = Properties.Resources.Dozer;
        }

        protected void moveRS()
        {
            if (loadedRS == -1) return;
            Global._rsData[loadedRS].setCenter(HaDesMath.movePointByDisAndAngle(center, angle, 3.5f));
            Global._rsData[loadedRS].setAngle(angle);
        }
        public void run(int _elapsed)
        {

            while (true)
            {
                processBreaking(_elapsed);
                int lastTime = runCommand(_elapsed);
                moveRS();
                if (status == Constants.cmdWaiting)
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
            if (status == Constants.cmdWaiting)
            {
                if (Global.waitingRoom.Count > 0 && Global._lcc[Global.waitingRoom[0]].getStatus() != Constants.cmdWaiting)
                {
                    loadRSToLCC(_curTime);
                    Global.movedSalvageRS++;
                    salvageNextRS(center, angle, _curTime + Global.LCCLoadingTime * 60);
                }
            }
        }

        public void prepareEFrameCommand()
        {
            int curTime = 0;
            int step = Global.EFrameBuiltTime * 3600 / 18;
            Command _cmd;
            int carried = 0;
            float c = HaDesMath.CalcDistancePoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos);
            PointF curPos = center, pos;
            float curAngle = angle;
            float alpha = HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos);
            int nextId;
            for (int i = Global.RSCounts; i > 0; i--)
            {
                if (carried == 3)
                {
                    break;
                }
                if (Global.isSalvage(i)) continue;

                if (carried == 0) // in first RS moving move up to RS
                {
                    // move up
                    pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.5f);
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    addCommand(_cmd);
                    curPos = pos;
                    curTime += step;
                }

                // move down
                pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f * carried - 4.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                addCommand(_cmd);
                curPos = pos;
                curTime += step;

                // rotate 90 degree
                _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle + 90, 1, i);
                addCommand(_cmd);
                curAngle += 90;
                curTime += step;

                // move to target position
                pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, alpha - 90, 1.0f + carried * 2.0f);
                pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 6.0f);
                _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, i);
                addCommand(_cmd);
                curPos = pos;
                curTime += step;

                if (carried == 2)
                {
                    // move to complete eframe position
                    pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, alpha - 90, 5.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 10.0f);
                    _cmd = new Command(curTime, Global.EFrameBuiltTime * 3600, curPos, pos, curAngle, curAngle, 0, -1);
                    addCommand(_cmd);
                    curPos = pos;
                    curTime = Global.EFrameBuiltTime * 3600;

                }
                else
                {
                    // move to next position
                    nextId = Global.getNextMoveId(i);
                    pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, alpha - 90, 1.0f + carried * 2.0f);
                    pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + 2.0f * (Global.RSCounts - nextId));
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    addCommand(_cmd);
                    curPos = pos;
                    curTime += step;

                    // rotate 90 degree
                    _cmd = new Command(curTime, curTime + step, curPos, curPos, curAngle, curAngle - 90, -1, -1);
                    addCommand(_cmd);
                    curAngle -= 90;
                    curTime += step;

                    // move up
                    pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 4.0f - 2.0f * carried);
                    _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
                    addCommand(_cmd);
                    curPos = pos;
                    curTime += step;
                }
                carried++;
            }
            makeFirstSalvageCommand(curPos, curAngle, curTime);
        }

        private void makeFirstSalvageCommand(PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime;
            if (Global._salvageRSList.Count == 0) return;
            int idx = Global._salvageRSList[0];
            Command _cmd;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng, d, alpha = HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos);

            pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, alpha - 90, 5.0f);
            pos = HaDesMath.movePointByDisAndAngle(pos, alpha - 180, 1.7f + (Global.RSCounts - idx) * 2.0f);

            d = HaDesMath.CalcDistancePoints(pos, curPos);
            int step = (int)(d * 3.6f / Global.DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, curAngle, curAngle, 0, -1);
            addCommand(_cmd);
            curPos = pos;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 20, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            addCommand(_cmd);
            curAngle -= 90;
            curTime += Global.SalvageTime * 20;

            // move down

            pos = HaDesMath.movePointByDisAndAngle(curPos, alpha - 90, -2);
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 20, curPos, pos, curAngle, curAngle, 0, idx);
            addCommand(_cmd);
            curPos = pos;
            curTime += Global.SalvageTime * 20;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 20, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            addCommand(_cmd);
            curAngle += 90;
            curTime += Global.SalvageTime * 20;

            gotoLCCStartPos(curPos, curAngle, curTime, idx);
        }

        private void loadRSToLCC(int lastTime)
        {
            Command _cmd;
            int step = (int)(5.0f * 3.6f / Global.LCCSpeed);
            foreach (int i in Global.waitingRoom)
            {
                _cmd = new Command(lastTime, lastTime + step, Global._lcc[i].getCenter(), new PointF(Global._lcc[i].getCenter().X, Global._lcc[i].getCenter().Y - 5), Global._lcc[i].getAngle(), Global._lcc[i].getAngle(), 0, -1);
                Global._lcc[i].addCommand(_cmd);
            }
            int idx = Global.waitingRoom[0];

            float alpha = HaDesMath.getAngleByPoints(Global.routeDozerPoints[Global.routeDozerPoints.Count - 1], Global.routeDozerPoints[Global.routeDozerPoints.Count - 2]);
            float beta = Global.curStorage == 0 ? HaDesMath.getAngleByPoints(Global.routeAPoints[0], Global.routeAPoints[1]) - 180 : HaDesMath.getAngleByPoints(Global.routeBPoints[0], Global.routeBPoints[1]) - 180;
            int dir;
            if (Global._lcc[idx].getAngle() < alpha) { dir = 1; }
            else { dir = -1; }
            PointF pos = Global.routeDozerPoints[Global.routeDozerPoints.Count - 1];
            _cmd = new Command(lastTime + step, lastTime + step + Global.LCCLoadingTime * 30, pos, pos, Global._lcc[idx].getAngle(), alpha, dir, -1);
            Global._lcc[idx].addCommand(_cmd);
            if (alpha < beta) { dir = 1; }
            else { dir = -1; }
            _cmd = new Command(lastTime + step + Global.LCCLoadingTime * 30, lastTime + step + Global.LCCLoadingTime * 60, pos, pos, alpha, beta, dir, loadedRS, Constants.cmdWaiting);
            Global._lcc[idx].addCommand(_cmd);
            setStatus(0);
        }

        private void salvageNextRS(PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime;
            PointF curPos = lastPos, pos;
            float curAngle = lastAng;
            Command _cmd;
            float d, alpha;
            int step;

            // move to next RS
            for (int i = Global.routeDozerPoints.Count - 2; i > 0; i--)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(curPos, Global.routeDozerPoints[i]);

                step = (int)(d * 3.6f / Global.DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeDozerPoints[i], alpha, alpha, 0, -1);
                addCommand(_cmd);
                curPos = Global.routeDozerPoints[i];
                curAngle = alpha;
                curTime += step;
            }
            if (Global.movedSalvageRS >= Global._salvageRSList.Count)
            {
                alpha = HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos);
                _cmd = new Command(curTime, curTime + 10, curPos, curPos, alpha, alpha, 0, -1);
                addCommand(_cmd);
                return;
            }
            int idx = Global._salvageRSList[Global.movedSalvageRS];

            pos = HaDesMath.movePointByDisAndAngle(Global._basePoint, HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos) - 90, 3.0f);
            pos = HaDesMath.movePointByDisAndAngle(pos, HaDesMath.getAngleByPoints(Global.initRSPos.StartPos, Global.initRSPos.EndPos) - 180, 1.7f + (Global.RSCounts - idx) * 2.0f);

            d = HaDesMath.CalcDistancePoints(pos, curPos);
            alpha = HaDesMath.getAngleByPoints(curPos, pos);
            step = (int)(d * 3.6f / Global.DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
            addCommand(_cmd);
            curPos = pos;
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 15, curPos, curPos, curAngle, curAngle - 90, -1, -1);
            addCommand(_cmd);
            curAngle -= 90;
            curTime += Global.SalvageTime * 15;

            // move up
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, 2.0f);
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 15, curPos, pos, curAngle, curAngle, 0, -1);
            addCommand(_cmd);
            curPos = pos;
            curTime += Global.SalvageTime * 15;

            // move down
            pos = HaDesMath.movePointByDisAndAngle(curPos, curAngle, -2.0f);
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 15, curPos, pos, curAngle, curAngle, 0, idx);
            addCommand(_cmd);
            curPos = pos;
            curTime += Global.SalvageTime * 15;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.SalvageTime * 15, curPos, curPos, curAngle, curAngle + 90, 1, idx);
            addCommand(_cmd);
            curAngle += 90;
            curTime += Global.SalvageTime * 15;

            gotoLCCStartPos(curPos, curAngle, curTime, idx);
        }

        private void gotoLCCStartPos(PointF lastPos, float lastAng, int lastTime, int idx)
        {
            if (Global.routeDozerPoints.Count < 2)
            {
                return;
            }
            int curTime = lastTime;
            PointF curPos = lastPos;
            float curAngle = lastAng;
            Command _cmd;
            float d = HaDesMath.CalcDistancePoints(curPos, Global.routeDozerPoints[1]);
            float alpha = HaDesMath.getAngleByPoints(Global.routeDozerPoints[1], curPos);

            int step = (int)(d * 3.6f / Global.DozerSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, Global.routeDozerPoints[1], alpha, alpha, 0, idx);
            addCommand(_cmd);
            curPos = Global.routeDozerPoints[1];
            curTime += step;

            for (int i = 2; i < Global.routeDozerPoints.Count; i++)
            {
                int type = (i == Global.routeDozerPoints.Count - 1) ? Constants.cmdWaiting : 0;
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeDozerPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeDozerPoints[i], curPos);
                step = (int)(d * 3.6f / Global.DozerSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeDozerPoints[i], alpha, alpha, 0, idx, type);
                addCommand(_cmd);
                curPos = Global.routeDozerPoints[i];
                curTime += step;
            }
        }
    }
}
