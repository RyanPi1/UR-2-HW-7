/*HW 7 Arduino Side
  Programmer: Ryan Pizzirusso
  Due Date: Oct. 5, 2016*/
  
#include<AFMotor.h>

//initialize motors
AF_DCMotor leftMotor(3);
AF_DCMotor rightMotor(4);

//array for recieved data
byte recievedRaw[2];
int recieved[2];

//speed constants
const int full = 255;
const int half = full / 2;

//speed variables
int totalWidth;
float leftPercent;
float rightPercent;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
}//end setup

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available() > 0){
    Serial.readBytes(recievedRaw, 2);
    //Serial.readBytesUntil(<character>, recievedRaw, 2);
    recieved[0] = int(recievedRaw[0]);
    recieved[1] = int(recievedRaw[1]);
    
    if(recieved[0] != 0 && recieved[1] != 0){
      totalWidth = recieved[0] + recieved[1]; //find total width

      //find percentages of each side.  note, percentages will be in decimal form
      leftPercent = recieved[0] / totalWidth;
      rightPercent = recieved[1] / totalWidth;

      //multiply by full speed and drive. note, each wheel needs to be multiplied by the opposite side's percentage
      leftMotor.setSpeed(full * rightPercent);
      rightMotor.setSpeed(full * leftPercent);
      
    }//end if no zeros
    
    else{
      leftMotor.setSpeed(0);
      leftMotor.run(RELEASE);

      rightMotor.setSpeed(0);
      rightMotor.setSpeed(RELEASE);
    }//end else
    
  }//end if (Serial.available() > 0)
}//end loop
