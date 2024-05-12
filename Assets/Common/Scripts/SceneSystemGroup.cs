using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

namespace DOTS.DOD
{
    public abstract partial class SceneSystemGroup : ComponentSystemGroup
    {
        protected abstract string SceneName{get;}
        protected bool isInit = false;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (isInit)
            {
                return;
            }
            var subScene = Object.FindObjectOfType<SubScene>();
            if (subScene != null)
            {
                Enabled = subScene.gameObject.scene.name == SceneName;
            }
            isInit = true;
        }
    }
}

