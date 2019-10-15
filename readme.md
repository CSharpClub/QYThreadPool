# 线程池的工具类
    任务类线程池

## 适用途径
>1、任务类程序中，例如 N 多种类的任务，又想对这些任务划分下各自可用线程数量时

>2、还不知道，知道了会举例说明


## 导入项目
>因为这是 git 项目，有版本管理的好处，直接复制粘贴，虽然不影响使用，但对后续维护带来不便，所以建议用子模块方式导入项目

### 初始化 git
>确保你的项目是 git 管理的项目，如果已经是了，那么请忽略这一步。如果不是，请先执行

>git init


### 导入子模块

>git submodule add git@github.com:fjqingyou/QYThreadPool.git src/QYThreadPool

>上方的 git@github.com:fjqingyou/QYThreadPool.git 是当前项目地址，但是您可以 fork 之后再从您自己的 git 下面下载，而 src/QYThreadPool 这个部分则是导入到项目中的哪个文件夹里面

>正常来说，这个时候，您的项目中，已经有这些代码文件了，而且还有版本管理支持，如果您用的是VS，可能需要自行在文件夹那边，把它拖到解决方案管理器中一下，它似乎没自动刷新！

## 参与贡献
>欢迎任何有益的贡献，有问题欢迎开启 Issue


## 示例代码 1
``` C#
using System;
using QY.ThreadPool;

/// <summary>
/// 测试线程池
/// </summary>
public class TestThreadPool{

    /// <summary>
    /// 测试方法
    /// </summary>
    public void Test(){
        //实例化一个线程池管理器
        ThreadPoolManager threadPoolManager = new ThreadPoolManager(100);

        //对线程池管理器做初始化
        threadPoolManager.Init();

        //创建各种任务用的对象池，用于分配每种任务最大线程数，实际总线程数受限于 threadPoolManager 的最大线程数
        ThreadPool decryptThreadPool = threadPoolManager.CreateThreadPool(20);//模拟解密线程池
        ThreadPool chatThreadPool = threadPoolManager.CreateThreadPool(10);//模拟聊天线程池

        //因为是模拟，所以就只加一个线程任务举例就好了，毕竟用法都一样

        //模拟解密
        decryptThreadPool.AddTask(()=>{
            //模拟客户端参数
            string str1 = "被加密过的字符串";

            //执行解密
            string str2 = this.SimulationDecrypt(str1);

            //模拟后续任务
            Console.WriteLine("解密完成，得到结果。现在可以正式讲解密后的数据派遣到其他线程执行了！" + str2);
        });//

        //模拟聊天
        chatThreadPool.AddTask(()=>{
            //模拟客户端参数
            string str1 = "客户端发的聊天内容";

            //执行解密
            this.SimulationChar(str1);

            //模拟后续任务
            Console.WriteLine("数据库记录完毕，可以将这个消息发给其他客户端了" + str1);
        });
    }

    /// <summary>
    /// 模拟解密
    /// </summary>
    private string SimulationDecrypt(string body){
        Console.WriteLine("模拟解密算法");
        return body;//直接返回了，解密方式就不举例说明了！
    }

    /// <summary>
    /// 模拟聊天
    /// </summary>
    /// <param name="body"></param>
    private void SimulationChar(string body){
        Console.WriteLine("模拟记录数据库");
    }
    
}
```

## 示例代码 2
``` C#
    
    static void Main (string[] args) {
        //设置线程池
        ThreadPoolManager threadPoolManager = new ThreadPoolManager(65535);
        ThreadPool.ThreadPool threadPool = threadPoolManager.CreateThreadPool(8192);
        ThreadPoolManager.defaultManager = threadPoolManager;
        ThreadPoolManager.defaultPool = threadPool;
        threadPoolManager.Init();

        //在程序中的其他任意位置，都可以通过
        // ThreadPoolManager.defaultManager 这个获取默认的线程池管理器
        // ThreadPoolManager.defaultPool 这个获取默认的线程池
    }
```