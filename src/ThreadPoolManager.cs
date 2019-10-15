using System.Collections.Generic;
using System.Threading;

namespace QY.ThreadPool{
    /// <summary>
    /// 线程池管理器
    /// 
    /// 用于管理线程池
    /// </summary>
    public class ThreadPoolManager {
        /// <summary>
        /// 获取或设置默认线程池管理器
        /// 
        /// 必须在自己项目中自行生成对象并设置后其他地方获取
        /// </summary>
        /// <value></value>
        public static ThreadPoolManager defaultManager{get; set;}

        /// <summary>
        /// 获取或设置默认线程池
        /// 
        /// 必须在自己项目中自行生成对象并设置后其他地方获取
        /// </summary>
        /// <value></value>
        public static ThreadPool defaultPool {get;set;}

        /// <summary>
        /// 线程同步锁
        /// </summary>
        /// <returns></returns>
        protected object lockObj = new object();

        /// <summary>
        /// 待机线程列表
        /// 因为需要频繁增加与删除，反而很少遍历查询，所以采用 LinkedList 方案
        /// </summary>
        /// <typeparam name="Thread"></typeparam>
        /// <returns></returns>
        protected LinkedList<ThreadEntity> idleThreadList = new LinkedList<ThreadEntity>();

        /// <summary>
        /// 线程池列表
        /// 因为查询频繁，增删反而很少，所以采用 List 方案
        /// </summary>
        /// <typeparam name="ThreadPool"></typeparam>
        /// <returns></returns>
        protected List<ThreadPool> threadPoolList = new List<ThreadPool>();

        /// <summary>
        /// 待机线程数量
        /// </summary>
        /// <value></value>
        public int idleCount{
            get{ return idleThreadList.Count; }
        }
        
        /// <summary>
        /// 最大线程数量
        /// </summary>
        private int maxThreadCount;

        /// <summary>
        /// 运行中的线程数量
        /// </summary>
        private int runningThreadCount;
        
        /// <summary>
        /// 线程同步
        /// </summary>
        /// <returns></returns>
        public AutoResetEvent autoEvent = new AutoResetEvent(false);


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
        /// 线程池管理器构造方法
        /// </summary>
        /// <param name="maxThreadCount">所有线程池最大线程数</param>
        public ThreadPoolManager(int maxThreadCount){
            this.maxThreadCount = maxThreadCount;
        }

        /// <summary>
        /// 创建线程池
        /// </summary>
        /// <param name="maxThreadCount"></param>
        /// <returns></returns>
        public ThreadPool CreateThreadPool(int maxThreadCount){
            ThreadPool threadPool = new ThreadPool(this, maxThreadCount);
            this.threadPoolList.Add(threadPool);
            return threadPool;
        }

        /// <summary>
        /// 初始化
        /// 
        /// 这个初始化方法，请有且仅一次调用
        /// </summary>
        public void Init(){
            Thread thread = new Thread(this.DoWork);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 通知执行工作
        /// </summary>
        public void NotifyWork(){
            autoEvent.Set();
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        public void DoWork() {
            for(;;){
                //等待一个消息
                autoEvent.WaitOne();

                lock(this.lockObj){
                    for(int i = 0; i < this.threadPoolList.Count; i++){
                        ThreadPool threadPool = this.threadPoolList[i];
                        if(!threadPool.isFullLoad){//如果这个线程还未满负载
                            threadPool.DoWork();//让线程执行工作
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 请求线程
        /// </summary>
        /// <returns></returns>
        public ThreadEntity RequestThread(){
            ThreadEntity threadEntity = null;
            lock(this.lockObj){
                if(this.idleCount > 0){//如果有空闲线程
                    threadEntity = this.idleThreadList.First.Value;
                    this.idleThreadList.RemoveFirst();
                }else{//如果没有空闲线程
                    if(!this.isFullLoad){//如果当前管理器还未满负载
                        threadEntity = new ThreadEntity();
                        threadEntity.init();
                        this.runningThreadCount++;
                    }
                }
            }

            return threadEntity;
        }

        /// <summary>
        /// 回收线程
        /// </summary>
        /// <param name="entity"></param>
        public void StoreThread(ThreadEntity entity){
            if(entity != null){//不信任态度
                lock(this.lockObj){
                    this.idleThreadList.AddLast(entity);
                }
            }
        }
        
    }
}