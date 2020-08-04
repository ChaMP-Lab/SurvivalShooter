using UnityEngine;

namespace CompleteProject
{
    public enum PointingMode
    {
        Mouse,
        Gamepad,
    }

    public class PlayerMovement : MonoBehaviour
    {
        public PointingMode pointingMode = PointingMode.Gamepad;

        Vector3 movement;                       // The vector to store the direction of the player's movement.
        Animator anim;                          // Reference to the animator component.
        Rigidbody playerRigidbody;              // Reference to the player's rigidbody.
        difficultyControl difficultyControl;    // Reference to the difficultyControl script.
        int floorMask;                          // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
        float camRayLength = 100f;              // The length of the ray from the camera into the scene.

        void Awake ()
        {
            // If there are no joysticks/gamepads, set pointer mode to mouse
            if(Input.GetJoystickNames().Length == 0)
            {
                pointingMode = PointingMode.Mouse;
            }

            // Create a layer mask for the floor layer.
            floorMask = LayerMask.GetMask ("Floor");

            // Set up references.
            anim = GetComponent <Animator> ();
            playerRigidbody = GetComponent <Rigidbody> ();
            difficultyControl = GameObject.Find("DifficultyController").GetComponent<difficultyControl>();
        }


        void FixedUpdate ()
        {
            // Store the input axes.
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // Move the player around the scene.
            Move (h, v);

            // Turn the player to face the mouse cursor.
            Turning ();

            // Animate the player.
            Animating (h, v);
        }


        void Move (float h, float v)
        {
            // Set the movement vector based on the axis input.
            movement.Set(h, 0f, v);
            
            // Normalise the movement vector and make it proportional to the speed per second.
            movement = movement.normalized * difficultyControl.playerSpeed * Time.deltaTime;

            // Prevent player from moving beyond map center
            Vector3 newPosition = transform.position + movement;

            // Move the player to it's current position plus the movement.
            playerRigidbody.MovePosition(newPosition);
        }

        void Turning ()
        { 
            Vector3 lookAt;
            if(pointingMode == PointingMode.Mouse)
            {
                // Create a ray from the mouse cursor on screen in the direction of the camera.
                Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

                // Create a RaycastHit variable to store information about what was hit by the ray.
                RaycastHit floorHit;

                // Perform the raycast and if it hits something on the floor layer...
                if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask))
                {
                    // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                    lookAt = floorHit.point - transform.position;
                }else{
                    return;
                }

                // Ensure the vector is entirely along the floor plane.
                lookAt.y = 0f;
            }else{
                float x = Input.GetAxisRaw("Right-Stick-X");
                float y = Input.GetAxisRaw("Right-Stick-Y");

                lookAt = new Vector3(x, 0, -y);

                if(lookAt.magnitude < .7){
                    // Fat dead-zone that allows the player to release the stick but maintain rotation
                    return;
                }
            }

            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
            // Set the player's rotation to this new rotation.
            playerRigidbody.MoveRotation(Quaternion.LookRotation(lookAt));
        }


        void Animating (float h, float v)
        {
            // Create a boolean that is true if either of the input axes is non-zero.
            bool walking = h != 0f|| v != 0f;

            // Tell the animator whether or not the player is walking.
            anim.SetBool ("IsWalking", walking);
        }
    }
}