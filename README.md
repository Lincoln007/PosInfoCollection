# Pos信息手机系统
> ( ＞﹏＜) 不好意思,临时快速短期私项... 没买服务,误闯请别"呵呵",谢谢啦~


* 全驱动特定目录文件扫描
* FTP上传
* 文件归档
* 定期归档清理
* 维护日志
* 系统服务

- - - - - - - - - -

## 环境
* Win XP或以上
* .Net Framework 3.5+ [MS官网下载](http://www.microsoft.com/zh-CN/download/details.aspx?id=21 ".Net Framework")

- - - - - - - - - -

## 代码目录

+ PosInfoCollectionService 
 * Ftp操作
 * 简单日志操作
 * 业务逻辑
 * 服务
 * 服务安装
+ PosInfoCollectionTest
 * 命令行测试
+ ServiceSetup
 * 服务安装打包
+ resource
 * 原始图片资源备份
+ test-bin
 * 测试用

- - - - - - - - - -

## 测试
* 下载test-bin内文件. (不要中文路径,当前目录需要可写[日志用].)
* ftp-tools里包含了2个FtpServer, FTPserver.exe无需安装, 方便测试用.
* config.xml为配置项, 配置参数, 需要按UTF-8编码保存(文件默认).

### 终端测试
* 启动Ftp服务, 创建Ftp的存档目录(/pos/incoming), 该目录需要写权限.
* 在任意驱动器创建需要扫描的目录(\mi\upload).
* 更新config.xml配置
* 打开PosInfoCollectionTest.exe
* 在扫描目录中放入文件, 终端会显示执行情况, logs目录下有执行日志.

### 注册服务
* 启动Ftp服务, 创建Ftp的存档目录(/pos/incoming), 该目录需要写权限.
* 在任意驱动器创建需要扫描的目录(\mi\upload).
* 更新config.xml配置
* 使用install-service.cmd注册服务(管理员UAC申请需要选择"是").
* 查看服务
 - 控制面板 -> 系统和安全 -> 管理工具 -> 服务
 - 或 控制面板 直接搜索"服务"
 - 如果注册成功, 可以找到"PosInfoCollection"服务, 并且状态为"运行", 启动类型是"自动".
* 在扫描目录中放入文件, 查看logs目录下的执行日志.
* uninstall-service.cmd可以卸载服务(管理员UAC申请需要选择"是").