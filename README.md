# Integration of a 2D Sprite in a 3D world
---
## Objects:
- Billboarding.
- Y Billboarding.
- Custom shadowmaps.
- Custom how character is lit.
---
# Billboarding.
## What is billboarding?
Billboarding is the most common way of integrating sprites in a 3D World. Billboarding is taking a quad and make it always face the camera.

So, how can we do that? Whell, we could just simply make the quad face the camera by using the `LookAt` method from Unity, like this:

```csharp
private void Update()
{
  transform.LookAt(Camera.main.transform);
  transform.Rotate(0, 180, 0);
}
```
But now we have some problems:
- What if the camera isn't the main camera?
- This code runs in CPU and are unoptimized;

Whithout even saying that this is **not** Billboarding.

As you can see in the picture:

![NotBillboard](Images/notbillboard.png)

the quad isn't actually facing the camera like a sprite would, this is actually just **rotating** towards the camera position.

---
## References

Character art: The Adventurer - Male. sScary. Available at: https://sscary.itch.io/the-adventurer-male

"What is billboarding and can/should it be used in 3D games to create special effects?" Game Development Stack Exchange. Available at: https://gamedev.stackexchange.com/questions/54871

