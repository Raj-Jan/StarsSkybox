# StarsSkybox

Simple project that utilizes directX11 c# wrapper (sharpDX) and uses it to simulate night sky.
A basic particle system was used to imitate stars on the sky. Sun textures is just a plane texture painted on the screen. 
Sun flare is another particle system following sun movement on the screen.
Because of desturbing behavion of perspective projection with high fov, spherical projection was used for starfield with made high fov more bearable.
I used database of ~7 000 real stars incuding their brightness, temperatures and classes, and recalculated physical data into screen size and rgb color.

A user can use Left Shift - Left Ctrl to manipulate zoom, and discover richness of night sky.

![Alt Text](result.gif)
