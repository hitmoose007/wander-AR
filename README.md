# Wander-AR
 
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Wander is a mobile application designed for indoor navigation  through the use of Augmented Reality, working on the principles of Computer Vision and Visual Positioning System (VPS), to provide on-screen directions for users in real-time.

## Features

- Login/Signup via Email: Default form for user authentication using Google sign-in.

![login](Assets/ImmersalSDK/Samples/Images/loginscreen.png)

- Environment Mapping: Surrounding area is scanned using pictures taken by a phone's camera to build a 3D point cloud model.

![environment mapping](Assets/ImmersalSDK/Samples/Images/mapping1.png) ![environment mapping](Assets/ImmersalSDK/Samples/Images/mapping2.png)

- Content Placement: Involves positioning virtual elements, like text and images, within the real environment through spatial anchoring and localization techniques. 

![content placement](Assets/ImmersalSDK/Samples/Images/contentplace1.png) ![content placement](Assets/ImmersalSDK/Samples/Images/contentplace2.png) ![content placement](Assets/ImmersalSDK/Samples/Images/contentplace3.png)

- AR Navigation:
  - Placing Virtual Waypoints: Real-time virtual pointers for precise guidance.

![ar navigation](Assets/ImmersalSDK/Samples/Images/navigation1.png) ![ar navigation](Assets/ImmersalSDK/Samples/Images/navigation2.png)

  - Navigation Pathfinding: Navigational Graph is created and shortest path is calculated using A* algorithim.

![ar navigation](Assets/ImmersalSDK/Samples/Images/navigation3.png) ![ar navigation](Assets/ImmersalSDK/Samples/Images/navigation4.png)

- Home Screen: Retrieves all private and public map information from the firestore database and allows users to add new maps to the collection.

![home screen](Assets/ImmersalSDK/Samples/Images/homescreen1.png) ![home screen](Assets/ImmersalSDK/Samples/Images/homescreen2.jpg)

 
## SDK & APIs Used

- Immersal SDK: It is a spatial mapping & visual positioning system. It allows merging digital content with the real world by precise localization of devices in the physical world. 
- GoogleSignIn API: Provides users easy and secure sign-in and sign-up, in an easy-to-implement package for developers.
- Firebase Auth: Provides backend services UI libraries to authenticate users to our app.

## Additional Information

## Technology

The project is built using the following technologies:

- Software: Unity
- Language: C#
- Backend: Firebase, ImmersalSDK

## License

Wander is licensed under the [MIT License](LICENSE).

## Contact



For questions or suggestions, please reach out to us at [moosathebutt@gmail.com](mailto:moosathebutt@gmail.com) or [aminawasif20@gmail.com](mailto:aminawasif20@gmail.com) .

---