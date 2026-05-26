// Arduino shelf custom feature for hamachi shell Part Studios.
//
// SCAD reference (antweight_reference_platform/spinbot_ant.scad lines 162-166):
//   module arduinoShelf() {
//       translate([46,19,15]) rotate([-21,0,175]) cube([20,38,2]);   // tilted shelf
//       translate([46,19,0])  rotate([0,0,175]) translate([0,13,0]) cube([20,2,10]);  // vertical strut
//   }
//
// SCAD `rotate([rx,ry,rz])` composes as Rz*Ry*Rx (applied to a column vector).
// For the shelf this is Rz(175)*Rx(-21); for the strut it is Rz(175).
//
// FS pattern (validated 2026-05-26, hamachi-uqc):
//   - Compute the rotated local frame in FS (shelfX/shelfY from cos/sin of Rx, Rz).
//   - Build the tilted plane via plane(origin, normal, xDir) — no opPlane needed.
//   - Sketch the rectangle in plane-local coords, skSolve, extrude NEW.
//   - opBoolean UNION with qAllNonMeshSolidBodies (NOT qEverything(EntityType.BODY) —
//     that grabs non-solid entities and fails BOOLEAN_INPUTS_NOT_SOLID).
//
// Use case: instantiate this in any shell Part Studio that needs the Arduino shelf
// (shell_no_weapon already has it; shell_with_weapon picked it up via hamachi-uqc).

FeatureScript 2909;
import(path : "onshape/std/geometry.fs", version : "2909.0");

annotation { "Feature Type Name" : "Arduino Shelf" }
export const arduinoShelf = defineFeature(function(context is Context, id is Id, definition is map)
    precondition {}
    {
        // --- SHELF (tilted plate, 20 x 38 x 2 mm) ---
        const rxShelf = -21 * degree;
        const rzShelf = 175 * degree;
        const cosRx = cos(rxShelf);
        const sinRx = sin(rxShelf);
        const cosRz = cos(rzShelf);
        const sinRz = sin(rzShelf);

        // Rz*Rx applied to standard axes:
        //   shelfX = Rz*Rx*(1,0,0) = (cosRz, sinRz, 0)
        //   shelfY = Rz*Rx*(0,1,0) = (-cosRx*sinRz, cosRx*cosRz, sinRx)
        const shelfX = vector(cosRz, sinRz, 0);
        const shelfY = vector(-cosRx * sinRz, cosRx * cosRz, sinRx);
        const shelfNormal = cross(shelfX, shelfY);
        const shelfOrigin = vector(46, 19, 15) * millimeter;

        var shelfSketch = newSketchOnPlane(context, id + "shelfSk", {
            "sketchPlane" : plane(shelfOrigin, shelfNormal, shelfX)
        });
        skRectangle(shelfSketch, "rect", {
            "firstCorner"  : vector(0,  0) * millimeter,
            "secondCorner" : vector(20, 38) * millimeter
        });
        skSolve(shelfSketch);

        extrude(context, id + "shelfExt", {
            "entities"      : qSketchRegion(id + "shelfSk"),
            "endBound"      : BoundingType.BLIND,
            "depth"         : 2 * millimeter,
            "operationType" : NewBodyOperationType.NEW
        });

        // --- STRUT (vertical box 20 x 2 x 10 mm after Rz=175 + inner Y=13 offset) ---
        const strutX = vector(cosRz, sinRz, 0);
        const strutNormal = vector(0, 0, 1);
        const strutOrigin = vector(46, 19, 0) * millimeter;

        var strutSketch = newSketchOnPlane(context, id + "strutSk", {
            "sketchPlane" : plane(strutOrigin, strutNormal, strutX)
        });
        // Strut footprint in local (Rz-rotated) frame after the inner Y=13 translate:
        skRectangle(strutSketch, "rect", {
            "firstCorner"  : vector(0,  13) * millimeter,
            "secondCorner" : vector(20, 15) * millimeter
        });
        skSolve(strutSketch);

        extrude(context, id + "strutExt", {
            "entities"      : qSketchRegion(id + "strutSk"),
            "endBound"      : BoundingType.BLIND,
            "depth"         : 10 * millimeter,
            "operationType" : NewBodyOperationType.NEW
        });

        // --- Union all solid bodies into one ---
        opBoolean(context, id + "merge", {
            "tools"         : qAllNonMeshSolidBodies(),
            "operationType" : BooleanOperationType.UNION
        });
    });
