using System.Drawing;

namespace RSSimulation
{
    class processSurfaceLCC
    {
        public static void run(int _elapsed)
        {
            if (Global._surfLcc.getStatus() == 3) // waiting to check there is RS in storage A or B
            {
                if (Global.curStorageInSurface == 0)
                {
                    int cSA = Global.checkRSinStorage(Global.rsInLocA);
                    int cSB = Global.checkRSinStorage(Global.rsInLocB);
                    if (cSA != -1)
                    {
                        carryRSFromLocA(cSA, Global._surfLcc.getCenter(), Global._surfLcc.getAngle(), _elapsed);
                    }
                    else
                    {
                        if (cSB != -1)
                        {
                            Global.curStorageInSurface = 1;
                            carryRSFromLocB(cSB, Global._surfLcc.getCenter(), Global._surfLcc.getAngle(), _elapsed);
                        }
                    }
                }
                else
                {
                    int cSA = Global.checkRSinStorage(Global.rsInLocA);
                    int cSB = Global.checkRSinStorage(Global.rsInLocB);
                    if (cSB != -1)
                    {
                        carryRSFromLocB(cSB, Global._surfLcc.getCenter(), Global._surfLcc.getAngle(), _elapsed);
                    }
                    else
                    {
                        if (cSA != -1)
                        {
                            Global.curStorageInSurface = 0;
                            carryRSFromLocA(cSA, Global._surfLcc.getCenter(), Global._surfLcc.getAngle(), _elapsed);
                        }
                    }
                }
            }

            int status = Global._surfLcc.runCommand(_elapsed, Global.SimulationSpeed * Global.timerInterval / 1000);
            if (Global._surfLcc.getLoadedRS() != -1)
            {
                int id = Global._surfLcc.getLoadedRS();
                Global._rsData[id].setCenter(HaDesMath.movePointByDisAndAngle(Global._surfLcc.getCenter(), Global._surfLcc.getAngle(), 2.5f));
                Global._rsData[id].setAngle(Global._surfLcc.getAngle());
            }
            if (status == 3)
            {
                Global._surfLcc.setStatus(status);
            }
        }

        private static void carryRSFromLocA(int p, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in surface route A
            for (int i = Global.routeSurfaceAPoints.Count - 1; i >= 0; i--)
            {
                PointF pos = i > 0 ? Global.routeSurfaceAPoints[i - 1] : HaDesMath.movePointByDisAndAngle(Global.locAPosEn, HaDesMath.getAngleByPoints(Global.locAPosEn, Global.locAPosSt), 5.0f * p);
                d = HaDesMath.CalcDistancePoints(curPos, pos);
                alpha = HaDesMath.getAngleByPoints(pos, curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
                Global._surfLcc.addCommand(_cmd);
                curPos = pos;
                curAngle = alpha;
                curTime += step;
            }

            // load RS
            _cmd = new Command(curTime, curTime + Global.LCCLoadingTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, -1);
            Global._surfLcc.addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += Global.LCCLoadingTime * 60;


            // move in surface route A: back(carrying)
            for (int i = 0; i < Global.routeSurfaceAPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeSurfaceAPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeSurfaceAPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeSurfaceAPoints[i], alpha, alpha, 0, Global.rsInLocA[p]);
                Global._surfLcc.addCommand(_cmd);
                curPos = Global.routeSurfaceAPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            int r = (Global.rsInSurface.Count / 15);
            int c = (Global.rsInSurface.Count % 15);

            // move down.
            d = 5.0f + 5.0f * r;
            alpha = -90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, Global.rsInLocA[p]);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move left
            d = 5.0f + 2.0f * c;
            alpha = 180;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, Global.rsInLocA[p]);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, Global.rsInLocA[p]);
            Global._surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, -1);
            Global._surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;


            // move right
            d = 5.0f + 2.0f * c;
            alpha = 0;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move up.
            d = 5.0f + 5.0f * r;
            alpha = 90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1, 3);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            Global.rsInSurface.Add(Global.rsInLocA[p]);
            Global.rsInLocA[p] = -1;
            Global._surfLcc.setStatus(0);
        }

        private static void carryRSFromLocB(int p, PointF lastPos, float lastAng, int lastTime)
        {
            int curTime = lastTime, step;
            PointF curPos = lastPos;
            float curAngle = lastAng, alpha, d;
            Command _cmd;

            // move in surface route B
            for (int i = Global.routeSurfaceBPoints.Count - 1; i >= 0; i--)
            {
                PointF pos = i > 0 ? Global.routeSurfaceBPoints[i - 1] : HaDesMath.movePointByDisAndAngle(Global.locBPosEn, HaDesMath.getAngleByPoints(Global.locBPosEn, Global.locBPosSt), 5.0f * p);
                d = HaDesMath.CalcDistancePoints(curPos, pos);
                alpha = HaDesMath.getAngleByPoints(pos, curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, pos, alpha, alpha, 0, -1);
                Global._surfLcc.addCommand(_cmd);
                curPos = pos;
                curAngle = alpha;
                curTime += step;
            }

            // load RS
            _cmd = new Command(curTime, curTime + Global.LCCLoadingTime * 60, curPos, curPos, curAngle, curAngle + 180, 1, -1);
            Global._surfLcc.addCommand(_cmd);
            curAngle = curAngle + 180;
            curTime += Global.LCCLoadingTime * 60;


            // move in surface route B: back(carrying)
            for (int i = 0; i < Global.routeSurfaceBPoints.Count; i++)
            {
                d = HaDesMath.CalcDistancePoints(curPos, Global.routeSurfaceBPoints[i]);
                alpha = HaDesMath.getAngleByPoints(Global.routeSurfaceBPoints[i], curPos);

                step = (int)(d * 3.6f / Global.LCCSpeed);
                _cmd = new Command(curTime, curTime + step, curPos, Global.routeSurfaceBPoints[i], alpha, alpha, 0, Global.rsInLocB[p]);
                Global._surfLcc.addCommand(_cmd);
                curPos = Global.routeSurfaceBPoints[i];
                curAngle = alpha;
                curTime += step;
            }

            int r = (Global.rsInSurface.Count / 15);
            int c = (Global.rsInSurface.Count % 15);

            // move down.
            d = 5.0f + 5.0f * r;
            alpha = -90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, Global.rsInLocB[p]);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move left
            d = 5.0f + 2.0f * c;
            alpha = 180;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, Global.rsInLocB[p]);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, Global.rsInLocB[p]);
            Global._surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;

            // rotate 90 degree
            _cmd = new Command(curTime, curTime + Global.LCCDropTime * 30, curPos, curPos, curAngle, curAngle + 90, 1, -1);
            Global._surfLcc.addCommand(_cmd);
            curAngle += 90;
            curTime += Global.LCCDropTime * 30;


            // move right
            d = 5.0f + 2.0f * c;
            alpha = 0;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            // move up.
            d = 5.0f + 5.0f * r;
            alpha = 90;
            step = (int)(d * 3.6f / Global.LCCSpeed);
            _cmd = new Command(curTime, curTime + step, curPos, HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d), alpha, alpha, 0, -1, 3);
            Global._surfLcc.addCommand(_cmd);
            curPos = HaDesMath.movePointByDisAndAngle(curPos, alpha + 180, d);
            curAngle = alpha;
            curTime += step;

            Global.rsInSurface.Add(Global.rsInLocA[p]);
            Global.rsInLocA[p] = -1;
            Global._surfLcc.setStatus(0);
        }
    }
}
