using System;
using System.Threading;

namespace QY.ThreadPool{
    /// <summary>
    /// 线程实体类
    /// </summary>
    public class QYThreadEntity{
        /// <summary>
        /// 移除这个实体的时间
        /// 
        /// 暂时还未用上，预留在线程管理工具中，自动释放使用的
        /// </summary>
        public long removeTime;

        /// <summary>
        /// 当前执行的任务
        /// </summary>
        private Action task;

        /// <summary>
        /// 执行结束后回调函数
        /// </summary>
        private Action callback;

        /// <summary>
        /// 线程同步
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent autoEvent = new AutoResetEvent(false);

        /// <summary>
        /// 初始化
        /// </summary>
        public void init(){
            Thread thread = new Thread(this.run);
            thread.IsBackground = true;
            thread.Start();
        }


        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="task">要执行的任务</param>
        /// <param name="callback">任务执行完毕后的回调函数</param>
        public void execute(Action task, Action callback){
            this.task = task;
            this.callback = callback;

            //通知可以执行任务了
            this.autoEvent.Set();
        }

        /// <summary>
        /// 线程实际执行
        /// </summary>
        protected void run(){
            for(;;){
                //等待下一个命令
                this.autoEvent.WaitOne();

                //执行任务
                try{
                    task();
                }catch (Exception e){
                    Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
                }finally{
                    task = null;//释放任务对象的引用
                }

                //回调函数
                if(callback != null){
                    try{
                        callback();
                    }catch(Exception e){
                        Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
                    }finally{
                        callback = null;//释放回调函数的引用
                    }
                }
            }
        }
    }
}