using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSimulation
{
    class SurfaceCarrier : Carrier
    {
        public SurfaceCarrier() : base() { }
        public SurfaceCarrier(int _id) : base(_id) { }

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
            int cSA, cSB;
            switch (status)
            {
                case Constants.cmdSurfaceWaiting:
                    if (Global._salvageRSList.Count == Global.rsInSurface.Count)
                    {
                        Global.simulationCompleted = true;
                        return;
                    }
                    if (Global.curStorageInSurface == 0)
                    {
                        cSA = Global.checkRSinStorage(Global.rsInLocA);
                        cSB = Global.checkRSinStorage(Global.rsInLocB);
                        if (cSA != -1)
                        {
                            gotoStorage(0, cSA, center, angle, _curTime);
                            setStatus(0);
                        }
                        else
                        {
                            if (cSB != -1)
                            {
                                Global.curStorageInSurface = 1;
                                gotoStorage(1, cSB, center, angle, _curTime);
                                setStatus(0);
                            }
                        }
                    }
                    else
                    {
                        cSA = Global.checkRSinStorage(Global.rsInLocA);
                        cSB = Global.checkRSinStorage(Global.rsInLocB);
                        if (cSB != -1)
                        {
                            gotoStorage(1, cSB, center, angle, _curTime);
                            setStatus(0);
                        }
                        else
                        {
                            if (cSA != -1)
                            {
                                Global.curStorageInSurface = 0;
                                gotoStorage(0, cSA, center, angle, _curTime);
                                setStatus(0);
                            }
                        }
                    }
                    break;
                case Constants.cmdLoadFromlocA:
                    cSA = Global.checkRSinStorage(Global.rsInLocA);
                    if (cSA != -1)
                    {
                        carryRSToSurface(0, cSA, center, angle, _curTime);
                        Global.rsInSurface.Add(Global.rsInLocA[cSA]);
                        Global.rsInLocA[cSA] = -1;
                    }
                    break;
                case Constants.cmdLoadFromlocB:
                    cSB = Global.checkRSinStorage(Global.rsInLocB);
                    if (cSB != -1)
                    {
                        carryRSToSurface(1, cSB, center, angle, _curTime);
                        Global.rsInSurface.Add(Global.rsInLocB[cSB]);
                        Global.rsInLocB[cSB] = -1;
                    }
                    break;
            }
        }

        private void gotoStorage(int route, int p, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;
            List<PointF> routePoints = route == 0 ? Global.routeSurfaceAPoints : Global.routeSurfaceBPoints;
            PointF posSt = route == 0 ? Global.locAPosSt : Global.locBPosSt;
            PointF posEn = route == 0 ? Global.locAPosEn : Global.locBPosEn;
            // move in surface route
            for (int i = routePoints.Count - 1; i >= 0; i--)
            {
                PointF pos = i > 0 ? routePoints[i - 1] : HaDesMath.movePointByDisAndAngle(posEn, HaDesMath.getAngleByPoints(posEn, posSt), 5.0f * p);
                d = HaDesMath.CalcDistancePoints(curPos, pos);
                alpha = HaDesMath.getAngleByPoints(pos, curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
                addCommand(_cmd);
                curPos = pos;
                curAngle = alpha;
                curTime += step;
            }

            // load RS
            _cmd = new Command(curTime, curTime + Global.LCCLoadingTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, -1, Constants.cmdLoadFromlocA + route);
            addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += Global.LCCLoadingTime * 60;
        }

        private void carryRSToSurface(int route, int p, PointF lastPos, float lastAng, int lastTime)
        {
            if (p < 0) return;
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;
            List<PointF> routePoints = route == 0 ? Global.routeSurfaceAPoints : Global.routeSurfaceBPoints;
            int rsId = route == 0 ? Global.rsInLocA[p] : Global.rsInLocB[p];
            // move in surface route: back(carrying)
            for (int i = 0; i < routePoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, routePoints[i]);
                alpha = HaDesMath.getAngleByPoints(routePoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, routePoints[i], alpha, alpha, 0, rsId);
                addCommand(_cmd);
                curPos = routePoints[i];
                curAngle = alpha;
                curTime += step;
            }

            int r = (Global.rsInSurface.Count / 15);
            int c = (Global.rsInSurface.Count % 15);

            // move down.
            d = 5.0f + 5.0f * r;
            alpha = -90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsId);
            addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move left
            d = 5.0f + 2.0f * c;
            alpha = 180;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, rsId);
            addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, rsId);
            addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, -1);
            addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;


            // move right
            d = 5.0f + 2.0f * c;
            alpha = 0;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1);
            addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move up.
            d = 5.0f + 5.0f * r;
            alpha = 90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1, Constants.cmdSurfaceWaiting);
            addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;
            setStatus(0);
        }
    }
}
