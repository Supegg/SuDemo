﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace SuUtil
{
    public enum ReportCode
    {
        Message,//black
        WarningMessage,//yellow
        Fail,//Red
        Pass//Blue
    }

    public abstract class TestBase
    {
        protected BackgroundWorker worker = null;
        //protected SqlHelper db = null;
        //protected DBFactroy db = null;
        //protected bool isUseDB = false;
        protected Stopwatch watch = null;//new Stopwatch();

        /// <summary>
        /// God of test
        /// </summary>
        /// <param name="worker"></param>
        public TestBase(BackgroundWorker worker ,Stopwatch watch)
        {
            this.worker = worker;
            this.watch = watch;
        }

        public void Run()
        {
            try
            {
                test();
            }
            catch(Exception ex)
            {
                report(ReportCode.Fail, ex.Message);
                return;
            }
            report(ReportCode.Pass, "Pass");
        }
       

        /// <summary>
        /// 重写test注意事项
        /// 1.只报告过程，不报告结果
        /// 2.顺利跑完流程，即认为Pass。
        /// 3.如内部需对结果进行判断，非True/Pass，都以异常形式抛出
        /// </summary>
        protected abstract void test();

        protected void report(ReportCode code, string info)
        {
            info = string.Format("{0}:\t{1}", DateTime.Now, info);
            worker.ReportProgress((int)code, info);
            Logger.Daily(info);
        }
    }
}
