﻿using ColossalFramework;
using ColossalFramework.Math;
using Harmony;
using System;
using System.Collections;
using UnityEngine;
using static RenderManager;

namespace ForestBrush
{
    [HarmonyPatch(typeof(TreeTool), "ApplyBrush")]
    public class ApplyBrushPatch
    {
        internal static float Angle = ToolsModifierControl.cameraController.m_currentAngle.x;

        internal static bool AngleChanged;

        internal static bool RotationTogglePressed()
        {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand));
        }

        static bool Prefix(TreeTool __instance, Randomizer ___m_randomizer, Vector3 ___m_mousePosition, bool ___m_mouseLeftDown, bool ___m_mouseRightDown, ToolController ___m_toolController)
        {
            if(ForestBrushes.instance.BrushPanel != null && ___m_mouseLeftDown)
            {
                ___m_toolController.SetBrush(null, Vector3.zero, 1f);
                if (__instance.m_prefab != null)
                {
                    for (var i = 0; i < 1024; i++)
                    {
                        var xz = UnityEngine.Random.insideUnitCircle;
                        var _x = xz.x;
                        var _y = xz.y;
                        var position = ___m_mousePosition + new Vector3(_x, 0f, _y) * (__instance.m_brushSize / 2);
                        if (UserMod.Settings.SquareBrush)
                        {
                            var radians = Angle * Mathf.Deg2Rad;
                            var c = Mathf.Cos(radians);
                            var s = Mathf.Sin(radians);
                            var brushRadius = new Vector2(__instance.m_brushSize, __instance.m_brushSize) * 0.5f;                            
                            var center = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
                            var corner = center * __instance.m_brushSize - brushRadius;
                            var translated  = corner - center;
                            xz.x = translated.x * c - translated.y * s;
                            xz.y = translated.y * c + translated.x * s;
                            xz += center;
                            _x = xz.x;
                            _y = xz.y;
                            position = ___m_mousePosition + new Vector3(_x, 0f, _y);
                        }
                        TreeInfo ___m_treeInfo;
                        if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
                        {
                            ___m_treeInfo = __instance.m_prefab;
                        }
                        else
                        {
                            ___m_treeInfo = __instance.m_prefab.GetVariation(ref ___m_randomizer);
                        }
                        
                        position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position, out float f, out float f2);
                        var spacing = UserMod.Settings.UseTreeSize ? ___m_treeInfo.m_generatedInfo.m_size.x / 2 : UserMod.Settings.Spacing;
                        if (Mathf.Max(Mathf.Abs(f), Mathf.Abs(f2)) < (float)___m_randomizer.Int32(10000u) * 5E-05f)
                        {
                            Randomizer randomizer = ___m_randomizer;
                            uint seed = Singleton<TreeManager>.instance.m_trees.NextFreeItem(ref randomizer);
                            Randomizer randomizer2 = new Randomizer(seed);
                            float num22 = ___m_treeInfo.m_minScale + (float)randomizer2.Int32(10000u) * (___m_treeInfo.m_maxScale - ___m_treeInfo.m_minScale) * 0.0001f;
                            float num23 = ___m_treeInfo.m_generatedInfo.m_size.y * num22;
                            float num24 = 4.5f;
                            Vector2 vector2 = VectorUtils.XZ(position);
                            Quad2 quad = default(Quad2);
                            quad.a = vector2 + new Vector2(-num24, -num24);
                            quad.b = vector2 + new Vector2(-num24, num24);
                            quad.c = vector2 + new Vector2(num24, num24);
                            quad.d = vector2 + new Vector2(num24, -num24);
                            Quad2 quad2 = default(Quad2);
                            quad2.a = vector2 + new Vector2(-spacing, -spacing);
                            quad2.b = vector2 + new Vector2(-spacing, spacing);
                            quad2.c = vector2 + new Vector2(spacing, spacing);
                            quad2.d = vector2 + new Vector2(spacing, -spacing);
                            float y = ___m_mousePosition.y;
                            float maxY = ___m_mousePosition.y + num23;
                            ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
                            if (!Singleton<PropManager>.instance.OverlapQuad(quad, y, maxY, collisionType, 0, 0))
                            {
                                if (!Singleton<TreeManager>.instance.OverlapQuad(quad2, y, maxY, collisionType, 0, 0u))
                                {
                                    if (!Singleton<NetManager>.instance.OverlapQuad(quad, y, maxY, collisionType, ___m_treeInfo.m_class.m_layer, 0, 0, 0))
                                    {
                                        if (!Singleton<BuildingManager>.instance.OverlapQuad(quad, y, maxY, collisionType, ___m_treeInfo.m_class.m_layer, 0, 0, 0))
                                        {
                                            if (!Singleton<TerrainManager>.instance.HasWater(vector2))
                                            {
                                                var scale = randomizer.Int32(160) / 10;
                                                if (Mathf.PerlinNoise(position.x * scale, position.y * scale) > 0.5)
                                                {
                                                    uint num25;
                                                    if (Singleton<TreeManager>.instance.CreateTree(out num25, ref ___m_randomizer, ___m_treeInfo, position, false))
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TreeTool), "RenderOverlay", new Type[] { typeof(CameraInfo) })]
    class RenderOverlayPatch
    {
        static bool Prefix(TreeTool __instance, ToolController ___m_toolController, ToolBase.ToolErrors ___m_placementErrors, Vector3 ___m_mousePosition, bool ___m_mouseRightDown, Randomizer ___m_randomizer, CameraInfo cameraInfo)
        {
            if(ForestBrushes.instance.BrushPanel == null || (___m_mouseRightDown && !ApplyBrushPatch.RotationTogglePressed()))
            {
                return true;
            }
            else
            {
                TreeInfo treeInfo = __instance.m_prefab;
                if (__instance.m_mode == TreeTool.Mode.Brush && treeInfo != null && !___m_toolController.IsInsideUI && Cursor.visible)
                {
                    var size = __instance.m_brushSize / 2;
                    Color toolColor = ToolsModifierControl.toolController.m_validColorInfo;
                    ___m_toolController.RenderColliding(cameraInfo, toolColor, toolColor, toolColor, toolColor, 0, 0);
                    ToolManager instance = Singleton<ToolManager>.instance;
                    instance.m_drawCallData.m_overlayCalls = instance.m_drawCallData.m_overlayCalls + 1;
                    if(UserMod.Settings.SquareBrush)
                    {
                        var Angle = ApplyBrushPatch.Angle;
                        var radians = Angle * Mathf.Deg2Rad;
                        var cos = Mathf.Cos(radians);
                        var sin = Mathf.Sin(radians);                       

                        Quad3 quad = default(Quad3);
                        Vector2 xz = VectorUtils.XZ(___m_mousePosition);
                        
                        var a = xz + new Vector2(-size, -size);
                        var aT = a - xz;
                        a.x = aT.x * cos - aT.y * sin;
                        a.y = aT.y * cos + aT.x * sin;
                        a += xz;


                        var b = xz + new Vector2(-size, size);
                        var bT = b - xz;
                        b.x = bT.x * cos - bT.y * sin;
                        b.y = bT.y * cos + bT.x * sin;
                        b += xz;

                        var c = xz + new Vector2(size, size);
                        var cT = c - xz;
                        c.x = cT.x * cos - cT.y * sin;
                        c.y = cT.y * cos + cT.x * sin;
                        c += xz;

                        var d = xz + new Vector2(size, -size);
                        var dT = d - xz;
                        d.x = dT.x * cos - dT.y * sin;
                        d.y = dT.y * cos + dT.x * sin;
                        d += xz;

                        quad.a = new Vector3(a.x, 0f, a.y);
                        quad.b = new Vector3(b.x, 0f, b.y);
                        quad.c = new Vector3(c.x, 0f, c.y);
                        quad.d = new Vector3(d.x, 0f, d.y);
                        Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, toolColor, quad, ___m_mousePosition.y - 100f, ___m_mousePosition.y + 100f, false, true);
                    }                        
                    else Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, toolColor, ___m_mousePosition, size * 2, ___m_mousePosition.y - 100f, ___m_mousePosition.y + 100f, false, true);
                }
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(TreeTool), "OnToolGUI")]
    public class OnToolGUIPatch
    {
        static bool Prefix(TreeTool __instance, ToolController ___m_toolController, bool ___m_mouseLeftDown, bool ___m_mouseRightDown, Event e)
        {
            if (ForestBrushes.instance.BrushPanel != null)
            {
                if (!___m_toolController.IsInsideUI && e.type == EventType.MouseDown)
                {
                    if (e.button == 0)
                    {
                        ___m_mouseLeftDown = true;
                        if (__instance.m_mode == TreeTool.Mode.Single)
                        {
                            Singleton<SimulationManager>.instance.AddAction((IEnumerator)typeof(TreeTool).GetMethod("CreateTree", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, null));
                        }
                    }
                    else if (e.button == 1)
                    {
                        ___m_mouseRightDown = true;
                        ApplyBrushPatch.AngleChanged = false;
                    }
                }
                else if (e.type == EventType.MouseUp)
                {
                    if (e.button == 0)
                    {
                        ___m_mouseLeftDown = false;
                    }
                    else if (e.button == 1)
                    {
                        ___m_mouseRightDown = false;
                        if (__instance.m_mode == TreeTool.Mode.Brush && !ApplyBrushPatch.AngleChanged)
                        {
                            Singleton<SimulationManager>.instance.AddAction(ToggleAngle());
                        }
                    }
                }
                return false;
            }
            else return true;
        }

        private static IEnumerator ToggleAngle()
        {
            ApplyBrushPatch.Angle = Mathf.Round(ApplyBrushPatch.Angle / 45f - 1f) * 45f;
            if (ApplyBrushPatch.Angle < 0f)
            {
                ApplyBrushPatch.Angle += 360f;
            }
            if (ApplyBrushPatch.Angle >= 360f)
            {
                ApplyBrushPatch.Angle -= 360f;
            }
            yield return 0;
            yield break;
        }
    }

    [HarmonyPatch(typeof(TreeTool), "OnToolUpdate")]
    public class OnToolUpdatePatch
    {
        static void Postfix(TreeTool __instance, ToolController ___m_toolController)
        {
            if (ForestBrushes.instance.BrushPanel != null)
            {
                if (__instance.m_mode == TreeTool.Mode.Brush && Input.GetKey(KeyCode.Mouse1) && ApplyBrushPatch.RotationTogglePressed())
                {
                    float axis = Input.GetAxis("Mouse X");
                    if (axis != 0f)
                    {
                        ApplyBrushPatch.AngleChanged = true;
                        Singleton<SimulationManager>.instance.AddAction(DeltaAngle(axis * 10f));
                    }
                }
            }
        }

        private static IEnumerator DeltaAngle(float delta)
        {
            ApplyBrushPatch.Angle += delta;
            yield return 0;
            yield break;
        }
    }
}