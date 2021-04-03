//
// Airlock.cs
// Copyright (c), 2021, Kirill GPRB.
//
// https://github.com/undnull/sescripts
//

//
// This script uses a bit deprecated way of setting up:
// 1. Place two doors and a single air vent.
// 2. Place a programmable block and load the script.
// 3. Optinally change blocks' names in the script.
// 4. Name the air vent and doors accordingly.
// 5. Recompile the script. Doors should close and turn off.
// 6. To open the inner door run the script with "inner" argument.
// 7. To open the outer door run the script with "outer" argument.
// 8. In case of an "emergency" run the script with "emerg" argument.
//    This will open both doors and depressurize the area.
// 9. Optinally place a bunch of button panels with script actions.
//

//
// Block names.
// This can be changed.
//
readonly string innerDoorName = "airlock_innerDoor";
readonly string outerDoorName = "airlock_outerDoor";
readonly string airVentName = "airlock_airVent";

IMyDoor innerDoor;
IMyDoor outerDoor;
IMyAirVent airVent;

bool innerRequest = false;
bool outerRequest = false;
bool emergRequest = false;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    innerDoor = GridTerminalSystem.GetBlockWithName(innerDoorName) as IMyDoor;
    outerDoor = GridTerminalSystem.GetBlockWithName(outerDoorName) as IMyDoor;
    airVent = GridTerminalSystem.GetBlockWithName(airVentName) as IMyAirVent;

    if(innerDoor == null || outerDoor == null || airVent == null)
        throw new NullReferenceException("Airlock can not work without two doors and one vent.\nPlease check the code.");

    innerDoor.Enabled = true;
    outerDoor.Enabled = true;
    innerDoor.CloseDoor();
    outerDoor.CloseDoor();
}

public void Main(string arg, UpdateType updateType)
{
    //
    // Update every 10 ticks
    //
    if(updateType == UpdateType.Update10) {
        //
        // Disable doors when fully opened or closed
        //
        if(innerDoor.Status == DoorStatus.Closed || innerDoor.Status == DoorStatus.Open)
            innerDoor.Enabled = false;
        if(outerDoor.Status == DoorStatus.Closed || outerDoor.Status == DoorStatus.Open)
            outerDoor.Enabled = false;

        //
        // Inner door request.
        // Waits for pressurization
        //
        if(innerRequest && (outerDoor.Status == DoorStatus.Closed) && (airVent.Status != VentStatus.Depressurized)) {
            innerDoor.Enabled = true;
            innerDoor.OpenDoor();
            innerRequest = false;
            return;
        }

        //
        // Outer door request
        //
        if(outerRequest && (innerDoor.Status == DoorStatus.Closed)) {
            outerDoor.Enabled = true;
            outerDoor.OpenDoor();
            outerRequest = false;
            return;
        }

        //
        // Emergency request
        // Opens both doors
        //
        if(emergRequest) {
            innerDoor.Enabled = true;
            innerDoor.OpenDoor();
            outerDoor.Enabled = true;
            outerDoor.OpenDoor();
            emergRequest = false;
            return;
        }
    }
    else {
        arg = arg.ToLower().Trim();

        //
        // Inner door request.
        //
        if(arg == "inner") {
            innerRequest = true;
            if(!outerDoor.Enabled) {
                outerDoor.Enabled = true;
                outerDoor.CloseDoor();
            }
            return;
        }

        //
        // Outer door request
        //
        if(arg == "outer") {
            outerRequest = true;
            if(!innerDoor.Enabled) {
                innerDoor.Enabled = true;
                innerDoor.CloseDoor();
            }
            return;
        }

        //
        // Emergency request
        //
        if(arg == "emerg") {
            emergRequest = true;
            return;
        }
    }
}
