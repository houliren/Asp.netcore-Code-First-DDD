# Asp.NetCoreCodeFirstAndDDD

#### 介绍
Asp.NetCore使用DDD+codefirst

#### 软件架构
 a.Panda.DynamicWebApi动态生成Api组件，为了把Controller从Api项目内解耦出来，如果直接拿出来，你会发现接口消失了，没有用了，这时候就需要使用该组件来动态生成Api了。

 b.Swagger接口管理组件，可以自动生成接口调试页面，以及接口描述。为了更方便的调试接口和管理接口这时候我们需要载入该组件，该组件完美兼容Panda.DynamicWebApi组件，支持动态生成的接口。

 c.AutoMapper 实体映射组件。该组件主要做Entity和Dto之间的相互转换来使用

 d.Microsoft.EntityFrameworkCore  该组件是efcore的核心组件

 e.Microsoft.EntityFrameworkCore.Design 该组件是efcore的核心组件

 f.Microsoft.EntityFrameworkCore.Tools  该组件是efcore的核心组件

 g.MySql.Data.EntityFrameworkCore  该组件是mysql对支持efcore的核心组件

 h.MySql.Data.EntityFrameworkCore.Design 该组件是mysql对支持efcore的核心组件
         



#### 使用说明
该文件时开发模板，下载即可使用
