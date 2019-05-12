/**
 * This sketch converts EPOC signals via OSC messages
 * to Arduino signals that move/turn a robot.
 */

import processing.serial.*;
import cc.arduino.*;
import controlP5.*;
import oscP5.*;
import netP5.*;

Arduino arduino;
//Serial arduino;
ControlP5 controlP5;
controlP5.Label label;
OscP5 oscP5;
PImage barcode;
PImage qrcode;
PFont font;

// Arduino pins and variables
int M1 = 3; // motor 1 enable
int M1_DIR = 12; // motor 1 direction
int M2 = 11; // motor 2 enable
int M2_DIR = 13; // motor 2 direction

// variables to store Cognitiv values
// need to be public for oscP5
public float push = 0;
public float pull = 0;
public float left = 0;
public float right = 0;

// actuator thresholds
// need to be public for controlP5
public float moveForward = 0.5;
public float moveBack = 0.5;
public float turnLeft = 0.5;
public float turnRight = 0.5;

int delaySliderValue = 100;

// appearance
int barColorUnder = color(255, 0, 0); // red
int barColorOver = color(22, 255, 113); // green

Slider slider1;

Knob knob1, knob2, knob3, knob4;

void setup() {
  size(480, 360);
  frameRate(30);
  font = loadFont("NanoSansSC.vlw");
  smooth();
  
  println(Arduino.list());
  arduino = new Arduino(this, Arduino.list()[1], 9600);
  //arduino = new Serial(this, Serial.list()[1], 9600);
  //printArray(Serial.list());
  
  // set up Arduino pins
  // configure all outputs off for now
  arduino.pinMode(M1, arduino.OUTPUT); arduino.digitalWrite(M1, arduino.LOW);
  arduino.pinMode(M1_DIR, arduino.OUTPUT); arduino.digitalWrite(M1_DIR, arduino.LOW);
  arduino.pinMode(M2, arduino.OUTPUT); arduino.digitalWrite(M2, arduino.LOW);
  arduino.pinMode(M2_DIR, arduino.OUTPUT); arduino.digitalWrite(M2_DIR, arduino.LOW);
  arduino.analogWrite(M1, 0); // motor 1 off
  arduino.analogWrite(M2, 0); // motor 2 off
  
  // start controlP5
  controlP5 = new ControlP5(this);
  // create slider
  slider1 = controlP5.addSlider("delaySliderValue", 0, 200, delaySliderValue, 140, 35, 200, 10);
  label = slider1.getCaptionLabel();
  label.set( "" );
  slider1.setNumberOfTickMarks(21);
  // create knobs
  knob1 = controlP5.addKnob("forward",0.0,1.0,moveForward,100,110,40);
  knob2 = controlP5.addKnob("back",0.0,1.0,moveBack,180,110,40);
  knob3 = controlP5.addKnob("left",0.0,1.0,turnLeft,260,110,40);
  knob4 = controlP5.addKnob("right",0.0,1.0,turnRight,340,110,40);
  
  //start oscP5, listening for incoming messages at port 7400
  //make sure this matches the port in Mind Your OSCs
  oscP5 = new OscP5(this, 7400);
  
  // plug the messages for the Cognitiv values
  oscP5.plug(this,"getPush","/COG/PUSH"); // forward
  oscP5.plug(this,"getPull","/COG/PULL"); // backward
  oscP5.plug(this,"getLeft","/COG/LEFT");
  oscP5.plug(this,"getRight","/COG/RIGHT");
}

void draw() {
  background(0);
  
  drawConstantGraphics();
  
  // draw metric bars
  drawBarGraph(push, moveForward, 110);
  drawBarGraph(pull, moveBack, 190);
  drawBarGraph(left, turnLeft, 270);
  drawBarGraph(right, turnRight, 350);
    
  // evaluate and enable or stop motors
  if(push >= moveForward) {
    moveForward();
  } else if(pull >= moveBack) {
    moveBack();
  } else if(left >= turnLeft) {
      turnLeft();
  } else if(right >= turnRight) {
    turnRight();
  } else {
    stopAll();
  }
  
  // reset Cognitiv variables before repeating loop
  push = 0; pull = 0; left = 0; right = 0;
  
  // pause before repeating loop
  delay(delaySliderValue);
}

public void getPush(float theValue) {
  push = theValue;
}

public void getPull(float theValue) {
  pull = theValue;
}

public void getLeft(float theValue) {
  left = theValue;
}

public void getRight(float theValue) {
  right = theValue;
}

public void forward(float theValue) {
  moveForward = theValue;
  println("moveForward changed to " + theValue);
}

public void back(float theValue) {
  moveBack = theValue;
  println("moveBack changed to " + theValue);
}

public void left(float theValue) {
  turnLeft = theValue;
  println("turnLeft to " + theValue);
}

public void right(float theValue) {
  turnRight = theValue;
  println("turnRight changed to " + theValue);
}

void drawConstantGraphics() {
  // border
  stroke(255);
  line(0, 0, width-1, 0);
  line(width-1, 0, width-1, height-1);
  line(width-1, height-1, 0, height-1);
  line(0, height-1, 0, 0);
  
  
  // triangles
  fill(255);
  noStroke();
  smooth();
  triangle(108,190,120,170,132,190); // push/forward
  triangle(200,190,188,170,212,170); // pull/back
  triangle(289,191,270,180,289,168); // left
  triangle(350,191,350,168,369,180); // right
  
  // text labels
  textFont(font);
  textAlign(CENTER);
  text("delay (milliseconds)", 240, 64);
  text("variable actuator", 240, 100);
  text("cognitiv values", 240, 325);
  
  // graph lines
  int i;
  for (i = 1; i <= 11; i++) {
    stroke(map(i, 1, 11, 255, 0));
    float yPos = map(i, 1, 11, 210, 310);
    line(110, yPos, 370, yPos);
  }
}

void drawBarGraph(float value1, float value2, int xPos) {
  noStroke();
  rectMode(CORNERS);
  if (value1 >= value2) {
    fill(barColorOver, 127);
  } else {
    fill(barColorUnder, 127);
  }
  rect(xPos, 310, xPos + 20, 310 - map(value1, 0.0, 1.0, 0, 100));
}

public void moveForward() {
  //arduino.write('F');
  arduino.digitalWrite(M1_DIR, arduino.LOW);
  arduino.digitalWrite(M2_DIR, arduino.LOW);
  arduino.analogWrite(M1, 255);
  arduino.analogWrite(M2, 255);
}

public void moveBack() {
  //arduino.write('B');
  arduino.digitalWrite(M1_DIR, arduino.HIGH);
  arduino.digitalWrite(M2_DIR, arduino.HIGH);
  arduino.analogWrite(M1, 255);
  arduino.analogWrite(M2, 255);
}

public void turnLeft() {
  //arduino.write('L');
  arduino.digitalWrite(M1_DIR, arduino.HIGH);
  arduino.digitalWrite(M2_DIR, arduino.LOW);
  arduino.analogWrite(M1, 255);
  arduino.analogWrite(M2, 255);
}

public void turnRight() {
  //arduino.write('R');
  arduino.digitalWrite(M1_DIR, arduino.LOW);
  arduino.digitalWrite(M2_DIR, arduino.HIGH);
  arduino.analogWrite(M1, 255);
  arduino.analogWrite(M2, 255);
}

public void stopAll() {
  //arduino.write('S');
  arduino.digitalWrite(M1_DIR, arduino.LOW);
  arduino.digitalWrite(M2_DIR, arduino.LOW);
  arduino.analogWrite(M1, 0);
  arduino.analogWrite(M2, 0);
}
