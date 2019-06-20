
# TexturePacker


### 1> [手动选择图片生成图集](./TexturePacker2/CombineSprites.cs)
选择的图片要设置可读写，选择是否去除图片多余透明可能会减少最终生成的图集大小。
使用方式 **Tools/CustomAtlas**


---
### 2> [仿制版 SpriteAtlas](./TexturePacker/)

图集编辑器，用于手动构建图集，仿Unity2017 SpriteAtlas


#### a, 创建.asset
![](https://github.com/garsonlab/TexturePacker/raw/master/Create.png)


#### b, 编辑.asset
可以直接拖入文件夹和图片，只能打包**.png**图片

![](https://github.com/garsonlab/TexturePacker/raw/master/Inspector.png)

选择保存路径，点击Packager按钮后即可生成图集

生成有两个文件，一个图集文件，已自动按照Unity方法进行切割。
第二个问预制文件，可直接在代码中调用


### 3> [更方便的查看UGUI系统图集工具](./ViewAtlas/ViewAtlas.cs)

更快更方便的筛选图集， **Tools/快速浏览图集**。替代Sprite Packer，快速切换图集
