// Adafruit Motor shield library
// copyright Adafruit Industries LLC, 2009
// this code is public domain, enjoy!

#include <SoftwareSerial.h>  // library to read Bluetooth from serial             
#include <AFMotor.h>  // library for DC motors

AF_DCMotor motor1(1); // right motor
AF_DCMotor motor2(2); // left motor

SoftwareSerial mySerial(0,1);
const int ledPin    = 13;  

unsigned long duration;
unsigned long startTime;
unsigned long stopTime;

int gRightWheelSpeed = 0;  // right wheel speed
int gLeftWheelSpeed = 0;   // left wheel speed


// BLUETOOTH MODULE
/********************************************************************************/
// Read the data coming in via Serial (i.e., from the Bluetooth module)
String readFromModule() {

  String msg = "";
   
  while(mySerial.available()) {
    digitalWrite(ledPin, HIGH);
    delay(10);                                    // For program stability
    int c = mySerial.read();
    digitalWrite(ledPin, LOW);
    msg += (char) c;  
  }
  delay(10);

  return msg;
}


// BUTTON PRESS FUNCTIONS:
/********************************************************************************/
/********************************************************************************/
void updateRightWheelSpeed(int theSpeed)
{  
  if (theSpeed > 0) motor1.run(BACKWARD);
  if (theSpeed <= 0) motor1.run(FORWARD);  
  motor1.setSpeed(abs(theSpeed));
}

/********************************************************************************/
void updateLeftWheelSpeed(int theSpeed)
{
  if (theSpeed < 0) motor2.run(BACKWARD);
  if (theSpeed >= 0) motor2.run(FORWARD);  
  motor2.setSpeed(abs(theSpeed));
}


/********************************************************************************/
void forwardButtonPress(void)
{
  Serial.println("\t forward arrow pressed"); 
  startTime = millis();
}

/********************************************************************************/
// update speeds of both motors based on how long the forward button was pressed
void forwardButtonRelease(void)
{
  Serial.println("\t forward arrow released");

  stopTime = millis();
  duration = stopTime - startTime;
  
  Serial.print("\t F Duration = ");
  Serial.println(duration); 

   if (duration < 100){ // short press stops robot;
      gRightWheelSpeed += 50; 
      gLeftWheelSpeed += 50;
      }
   else{   // long press is full speed ahead;
      gRightWheelSpeed = 500; 
      gLeftWheelSpeed = 500;
      }
    
  updateRightWheelSpeed(gRightWheelSpeed);
  updateLeftWheelSpeed(gLeftWheelSpeed);
}

/********************************************************************************/
void reverseButtonPress(void)
{
  Serial.println("\t reverse arrow pressed");
  startTime = millis();
}

/********************************************************************************/
// update speeds of both motors based on how long the reverse button was pressed
void reverseButtonRelease(void)
{
  Serial.println("\t reverse arrow released");

  stopTime = millis();
  duration = stopTime - startTime;
  
  Serial.print("\t R Duration = ");
  Serial.println(duration); 
  
  if (duration < 100){ // short press stops robot;
      gRightWheelSpeed = 0; 
      gLeftWheelSpeed = 0;
      }
   else{   // long press is full speed reverse;
      gRightWheelSpeed = -500; 
      gLeftWheelSpeed = -500;
      }
    
  updateRightWheelSpeed(gRightWheelSpeed);
  updateLeftWheelSpeed(gLeftWheelSpeed);
}

/********************************************************************************/
void rightButtonPress(void)
{
  Serial.println("\t right arrow pressed");

  int rightTemp = round(gRightWheelSpeed * .1); // slow down right wheel
  int leftTemp = round(gLeftWheelSpeed * 1.3); // speed up left wheel
  
  updateRightWheelSpeed(rightTemp);
  updateLeftWheelSpeed(leftTemp); 
}

/********************************************************************************/
// return to wheel speeds before button press
void rightButtonRelease(void)
{
  Serial.println("\t right arrow released");  

  // return to pre-turn wheel speeds
  updateRightWheelSpeed(gRightWheelSpeed);
  updateLeftWheelSpeed(gLeftWheelSpeed);  
}

/********************************************************************************/
// turn vehicle left
void leftButtonPress(void)
{
  Serial.println("\t left arrow pressed");

  int rightTemp = round(gRightWheelSpeed * 1.3); // speed up right wheel
  int leftTemp = round(gLeftWheelSpeed * .1); // slow down left wheel
 
  // do error checking here on temp speeds...
  updateRightWheelSpeed(rightTemp);
  updateLeftWheelSpeed(leftTemp);  
}

/********************************************************************************/
// return to wheel speeds before button press
void leftButtonRelease(void)
{
  Serial.println("\t left arrow released");
  
  updateRightWheelSpeed(gRightWheelSpeed);
  updateLeftWheelSpeed(gLeftWheelSpeed);
  
}


/********************************************************************************/
/********************************************************************************/
void setup() {
  mySerial.begin(9600);
  Serial.begin(9600);                     // You can run the serial monitor at the same time

  // turn on motor
  motor1.setSpeed(0);
  motor2.setSpeed(0);
  
  motor1.run(RELEASE);
  motor2.run(RELEASE);
}

/********************************************************************************/
// Main program loop
void loop() {

  delay(1000); // wait for a sec for everything to turn on
  String message;
  
  // Check for a Bluetooth message
  message = readFromModule();
  
  // If a message was received, check the message and act accordingly
  while (message.length() > 0) {

      char m = message.charAt(0);
      message.remove(0,1);

//      Serial.println("\t command: ??");

      // FORWARD BUTTON      
      if( m == 'U') forwardButtonPress();
      else if (m == 'u') forwardButtonRelease();

      // REVERSE BUTTON
      else if( m == 'D') reverseButtonPress();
      else if (m == 'd') reverseButtonRelease();
        
      // RIGHT TURN BUTTON  
      else if( m == 'R') rightButtonPress();
      else if (m == 'r') rightButtonRelease();

      // LEFT TURN BUTTON
      else if( m == 'L') leftButtonPress();
      else if (m == 'l') leftButtonRelease();

      // THIS WILL ACTIVE THE BALLOON STABBER DEVICE:
//      else if (m == 'M') servoTilt.write(60);
//      else if (m == 'm') servoTilt.write(120);
  }       
  
  delay(10); // give a little time for stuff to percolate  

}



/********************************************************************************/

/*
void loop() {
  uint8_t leftSpeed;
    uint8_t rightSpeed;
  leftSpeed = 500;
  rightSpeed = 500;
  delay(500);
  
  motor1.run(FORWARD);
  motor2.run(BACKWARD);
    motor1.setSpeed(leftSpeed);  
    motor2.setSpeed(rightSpeed);  
 
  delay(1000);
    motor1.setSpeed(0);  
    motor2.setSpeed(0);  
   
  delay(500);
    
  motor1.run(BACKWARD);
  motor2.run(FORWARD);

    motor1.setSpeed(leftSpeed);  
    motor2.setSpeed(rightSpeed);  
    
 delay(1000);
    motor1.setSpeed(0);  
    motor2.setSpeed(0);  
   
  delay(1000);

 
 
 }
 
// 
//  for (i=355; i!=0; i++) {
//    motor1.setSpeed(i);  
//    motor2.setSpeed(i);  
//    motor3.setSpeed(i);  
//    motor4.setSpeed(i);  
//    delay(10);
// }
 
*/
