# 代码说明

以下按照代码文件分别介绍一下每个类的功能。

## Models

Models中的代码是数据模型，用来存放数据。所有与TV功能相关的数据传递都通过这些类来完成。
- `Stations`: 不同视频源网站爬虫的具体实现
  - `ANAU`: 安徽农大视频源，节目非常丰富，但是爬虫也比较难写
  - `THU`: 清华视频源
  - `NEU`: 东北大学视频源以及回放源
- `Channel`: 频道类，存放频道名称等信息以及提供名称转换的功能。频道本身是**与视频源无关**的。
- `ITelevisionStation`: 代表视频源网站的接口。
  - `IScheduleStation`: 代表能提供节目单的网站的接口。
  - `IPlaybackStation`: 代表能提供回放的网站的接口
- `MultisourceProgram`: 用来存放同一个节目的多个视频源，相当于多个`ProgramSource`的集合。
- `Program`: 用来存放节目相关信息，**与频道有关但是与视频源无关**。
- `ProgramSource`: 存放节目的视频源。
- `TelevisionStationBase`: 继承自`ITelevisionStation`，提供缓存功能的基本实现（还没写）

## MVVM

这个文件夹里面的代码是实现MVVM架构本身需要的必要类。
- `BindableBase`: 所有被XAML绑定的类都需要继承这个类，并且利用`Set()`函数进行属性更新才能实现绑定的效果。
- `DelegateCommand`: `ICommand`接口的一个简单实现。`ICommand`接口在`Button`控件和`InvokeCommandAction`操作类中被用到。
- `DependencyBindableBase`: XAML中的元素都是`DependencyObject`对象。这个类就提供可供绑定的`DependencyObject`实现
- `ViewModelBase`: 给ViewModel提供基类，里面实现一些常用或者共用的功能

## Services

与功能实现相关的代码都需要写成服务类。服务类本身都是Singleton，通过`Instance`属性来进行访问，不要用`new`新建实例。
- `CollectionService`: 提供收藏相关功能的实现
- `NavigationService`: 界面跳转的实现
- `TelevisionService`: 提供电视节目获取与节目单获取等的统一接口。

以下是预计还需要实现的服务类，请参照这个实现
- `NotificationService`: 提供与系统提醒相关的功能
- `CalendarService`: 提供与日历集成相关的代码实现
- `RecordingService`: 提供与录播相关的代码实现。注意录播要实现成后台任务。
- `SettingService`: 提供更改设置的相关函数。应对UWP自带的Setting API做一些封装。

## Styles
这个文件夹里面存的是UI设计的样式，都是.xaml文件，里面是`ResourceDictionary`。

## Utils
这个文件夹里的代码是提供一些常用的代码，与功能实现本身无关。
- `Async`: 提供将异步代码进行同步调用的功能。直接利用`Task.Wait()`函数是会有问题的。
- `Connection`: 提供测试网络连接的函数
- `Debug`: 提供与调试相关的函数。在里面的`#if DEBUG`之后写的代码将会在UI打开之前进行运行。在`#else`之前打上断点就可以很方便地调试你的代码~但注意，`DebugMethod`中的代码不要commit上来。
- `Encoding`: 提供UWP中的GB2312编码。UWP本身是不带GB2312编码的。

## ViewModels
这个里面的代码是UI背后的逻辑实现，每一个View应该对应一个ViewModel，但ViewModel是可以对应多个View的。

## View
这里面就是UI设计的具体内容了。UI代码全部写在`.xaml`里面，对应的`.xaml.cs`称作Code-Behind，里面最好不要写任何代码。