## GetHolidays
#### 简介
>##### 这个项目是个Window窗口应用程序，使用爬虫来获取网络上的法定节假日信息，从而保存进数据库(SQLite)。之后根据节假日信息得出工作日时间，满足某些特定场合对时间的需求。


#### 文件结构
>SetupProject //打包项目   
>SQLLibrary  //后期打算放一些数据库封装    
>>Class1.cs  //暂时为空  
>  
>WindowFormCalendarService     
>>Form1.cs    //主窗口，负责爬虫，保存进数据库     
>>DataHolidaysForm.cs   // 子窗口，负责展示数据库现有数据   
> 
>WindowsFormCalendar     
>>WindowsFormCalendar.sln            
#### 更新日志
>##### 2017-08-07 上传了简单的demo，实现爬虫保存数据，展示现有数据库内容
>##### 2017-08-08 新加了打包项目，更改了SQlite的配置
