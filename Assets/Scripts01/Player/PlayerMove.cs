using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //the speed going in the correct direction
    public float moveSpeed = -3;
    public float leftRightSpeed = 4;

    // Update is called once per frame
    void Update()
    {
        //moving player forward
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);

        //making player move with keys
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            //if at boundry cant move past it.
            if (this.gameObject.transform.position.x > LevelBoundry.leftSide)
            {
                transform.Translate(Vector3.left * Time.deltaTime * leftRightSpeed);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.gameObject.transform.position.x < LevelBoundry.rightSide)
            {
                transform.Translate(Vector3.left * Time.deltaTime * leftRightSpeed * -1);
            }
        }
    }
}
