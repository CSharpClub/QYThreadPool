using System;
using System.Collections.Generic;

namespace QY.ThreadPool{
    /// <summary>
    /// 线程池
    /// </summary>
    public class QYThreadPool{
        /// <summary>
        /// 线程同步锁
        /// </summary>
        /// <returns></returns>
        protected object lockObj = new object();

        /// <summary>
        /// 线程池管理
        /// </summary>
        private QYThreadPoolManager manager;

        /// <summary>
        /// 最大线程数量
        /// </summary>
        private int maxThreadCount;

        /// <summary>
        /// 执行中的线程数量
        /// </summary>
        private int runningThreadCount;
        
        /// <summary>
        /// 等待执行的任务列表
        /// </summary>
        /// <typeparam name="Action"></typeparam>
        /// <returns></returns>
        private LinkedList<Action> taskList = new LinkedList<Action>();

        /// <summary>
        /// 是否处于满负载运行的状态
        /// 
        /// 也就是在线程池中执行的线程数量，是否已经达到最大线程数限制了
        /// </summary>
        /// <value></value>
        public bool isFullLoad{
            get{ return this.runningThreadCount == this.maxThreadCount; }
        }

        /// <summary>
        /// 线程池
        /// 
        /// 注意：
        /// 
        /// 请不要直接通过这个构造函数创建，请通过 ThreadPoolManager 的 CreateThreadPool 方法创建。
        /// 
        /// 否则你需要每次任务变更都需要自行调用 DoWork 方法。
        /// 
        /// 而如果是用上述方法创建的实例，则这些事情会自动完成。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="maxThreadCount"></param>
        public QYThreadPool(QYThreadPoolManager manager, int maxThreadCount){
            this.manager = manager;
            this.maxThreadCount = maxThreadCount;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(Action task){
            //有效数据监测
            if(task == null){
                throw new System.NullReferenceException("task is null");
            }

            lock(this.lockObj){
                //将任务添加到队列，等待启动
                this.taskList.AddLast(task);
            }

            //通知有任务变化
            this.manager.NotifyWork();
        }

        /// <summary>
        /// 执行线程池工作
        /// </summary>
        public void DoWork(){
            QYThreadEntity entity = null;
            Action task = null;
            lock(this.lockObj){
                //每次仅仅提取一个任务出来，有多个任务时，等待下次循环再获取
                //避免某个线程池任务堆满了管理器中的可用名额，使得其他线程池无法尽快得到执行的权限这种情况
                if(this.taskList.Count > 0){
                    entity = this.manager.RequestThread();
                    if(entity != null){
                        //提取出第一个
                        task = this.taskList.First.Value;
                        this.taskList.RemoveFirst();
                        
                        //累加计数器
                        this.runningThreadCount++;

                        //执行任务
                        entity.execute(task, ()=>{
                            lock(this.lockObj){
                                //回收线程资源
                                this.manager.StoreThread(entity);

                                //回收一个计数器
                                this.runningThreadCount--;
                            }

                            //通知有任务变化
                            this.manager.NotifyWork();
                        });
                    }
                }
            }
        }
    }
}
