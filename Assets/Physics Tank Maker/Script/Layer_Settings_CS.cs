using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChobiAssets.PTM
{

    public class Layer_Settings_CS
    {
        const int maxLayersNum = 14;
        public const int Wheels_Layer = 9; // cho bánh xe.
        public const int Reinforce_Layer = 10; // cho các cánh tay treo và các đối tượng gia cố bản lề đường ray. (Bỏ qua tất cả va chạm)
        public const int Body_Layer = 11; // cho MainBody (thân xe).
        public const int Bullet_Layer = 12; // cho viên đạn.
        public const int Armor_Collider_Layer = 13; // cho "Armor_Collider" và "Track_Collider" (va chạm với giáp và bánh xe).
        public const int Extra_Collider_Layer = 14; // cho các Collider bổ sung.

        // Cài đặt Layer Mask.
        public const int Layer_Mask = ~((1 << 2) + (1 << Reinforce_Layer) + (1 << Bullet_Layer) + (1 << Extra_Collider_Layer)); // Bỏ qua "Layer 2(Ignore Ray)", "Reinforce_Layer", "Bullet_Layer", "Extra_Collider_Layer".
        public const int Aiming_Layer_Mask = ~((1 << 2) + (1 << Wheels_Layer) + (1 << Reinforce_Layer) + (1 << Bullet_Layer) + (1 << Extra_Collider_Layer)); // Bỏ qua "Layer 2(Ignore Ray)", "Wheels_Layer", "Reinforce_Layer", "Bullet_Layer", "Extra_Collider_Layer".
        public const int Anti_Slipping_Layer_Mask = ~((1 << 2) + (1 << Reinforce_Layer) + (1 << Body_Layer) + (1 << Extra_Collider_Layer)); // Bỏ qua "Layer 2(Ignore Ray)", "Reinforce_Layer", "Body_Layer", "Extra_Collider_Layer".
        public const int Detect_Body_Layer_Mask = 1 << Body_Layer; // Chỉ va chạm với "Body_Layer" (được sử dụng để phát hiện một chiếc xe tăng).

        public static void Layers_Collision_Settings()
        {
            // Cài đặt va chạm cho "Wheels_Layer".
            Physics.IgnoreLayerCollision(Wheels_Layer, Wheels_Layer, true); // Bánh xe không va chạm với nhau.
            Physics.IgnoreLayerCollision(Wheels_Layer, Body_Layer, true); // Bánh xe không va chạm với MainBody.

            // Cài đặt va chạm cho "Reinforce_Layer".
            for (int i = 0; i <= maxLayersNum; i++)
            {
                Physics.IgnoreLayerCollision(Reinforce_Layer, i, true); // Cánh tay treo và các đối tượng gia cố bản lề đường ray không va chạm với tất cả.
            }

            // Cài đặt va chạm cho "Bullet_Layer".
            Physics.IgnoreLayerCollision(Bullet_Layer, Bullet_Layer, true); // Đạn không va chạm với nhau.
            Physics.IgnoreLayerCollision(Bullet_Layer, Wheels_Layer, true); // Đạn không va chạm với bánh xe.

            // Cài đặt va chạm cho "Armor_Collider_Layer".
            for (int i = 0; i <= maxLayersNum; i++)
            {
                Physics.IgnoreLayerCollision(Armor_Collider_Layer, i, true);
            }
            Physics.IgnoreLayerCollision(Armor_Collider_Layer, Bullet_Layer, false); // "Armor_colliders" va chạm chỉ với đạn.

            // Cài đặt va chạm cho "Extra_Collider_Layer".
            for (int i = 0; i <= maxLayersNum; i++)
            {
                Physics.IgnoreLayerCollision(Extra_Collider_Layer, i, true);
            }
            Physics.IgnoreLayerCollision(Extra_Collider_Layer, Extra_Collider_Layer, false); // Các Collider bổ sung chỉ va chạm với nhau.
        }

    }

}
