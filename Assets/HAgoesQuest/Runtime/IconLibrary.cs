using System.Collections.Generic;
using UnityEngine;
using HAgoesQuest.Core;

namespace HAgoesQuest
{
    // Original geometric line icons; drawn at runtime without external icon fonts.
    public static class IconLibrary
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        public static string ForEntity(HaEntity entity)
        {
            string domain = EntityPolicy.Domain(entity.Id);
            switch (entity.DeviceClass)
            {
                case "temperature": return "temperature";
                case "humidity": case "moisture": return "water";
                case "battery": return "battery";
                case "power": case "energy": case "voltage": return "energy";
                case "door": case "window": case "opening": return "door";
                case "motion": case "occupancy": case "presence": return "motion";
                case "illuminance": return "sun";
                case "smoke": case "gas": case "carbon_monoxide": return "alarm";
            }
            switch (domain)
            {
                case "light": return "lamp";
                case "switch": case "input_boolean": return "switch";
                case "camera": return "camera";
                case "image": return "image";
                case "media_player": return "speaker";
                case "climate": return "temperature";
                case "fan": return "fan";
                case "cover": return "cover";
                case "lock": return "lock";
                case "vacuum": return "vacuum";
                case "weather": return "sun";
                case "person": case "device_tracker": return "person";
                case "alarm_control_panel": return "alarm";
                case "scene": case "script": case "automation": return "automation";
                default: return "sensor";
            }
        }
        public static Sprite Get(string name)
        {
            if (cache.TryGetValue(name, out var result)) return result;
            const int size = 96; var pixels = new Color[size * size];
            void Line(float x1, float y1, float x2, float y2, float width = 2.1f)
            {
                var a = new Vector2(x1, y1); var d = new Vector2(x2-x1, y2-y1);
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x,y); float t = d.sqrMagnitude > 0 ? Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude) : 0;
                    float alpha = Mathf.Clamp01(width+0.5f-Vector2.Distance(p,a+d*t));
                    pixels[y*size+x] = new Color(1,1,1,Mathf.Max(pixels[y*size+x].a,alpha));
                }
            }
            void Circle(float cx,float cy,float r)
            {
                for(int i=0;i<40;i++) { float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20; Line(cx+r*Mathf.Cos(a),cy+r*Mathf.Sin(a),cx+r*Mathf.Cos(b),cy+r*Mathf.Sin(b)); }
            }
            void Box(float l,float b,float r,float t) { Line(l,b,r,b);Line(r,b,r,t);Line(r,t,l,t);Line(l,t,l,b); }
            switch(name)
            {
                case "lamp": Circle(48,57,21);Line(36,39,39,26);Line(60,39,57,26);Line(39,26,57,26);Line(41,19,55,19);break;
                case "switch": Circle(48,44,26);Line(48,45,48,78,4);break;
                case "temperature": Circle(48,27,12);Line(40,36,40,74);Line(56,36,56,74);Line(40,74,56,74);Line(48,28,48,62);Line(62,59,72,59);break;
                case "water": Line(48,79,26,44);Line(48,79,70,44);Line(26,44,27,29);Line(70,44,69,29);Line(27,29,39,19);Line(39,19,57,19);Line(57,19,69,29);break;
                case "battery": Box(21,29,72,67);Box(72,40,78,56);Line(30,38,30,58,3);Line(40,38,40,58,3);Line(50,38,50,58,3);break;
                case "energy": Line(55,81,29,46);Line(29,46,48,46);Line(48,46,40,16);Line(40,16,68,53);Line(68,53,49,53);Line(49,53,55,81);break;
                case "camera": Box(20,29,77,65);Circle(49,47,13);Line(31,65,37,75);Line(37,75,59,75);Line(59,75,65,65);break;
                case "image": Box(18,22,78,74);Circle(61,59,6);Line(22,27,40,52);Line(40,52,53,37);Line(53,37,65,45);Line(65,45,75,28);break;
                case "speaker": Box(19,36,34,60);Line(34,60,52,75);Line(52,75,52,21);Line(52,21,34,36);Line(64,32,71,40);Line(71,40,71,56);Line(71,56,64,64);break;
                case "cover": Box(22,18,74,78);for(int i=0;i<5;i++)Line(26,30+i*8,70,30+i*8);break;
                case "door": Box(27,16,69,80);Circle(59,45,2);break;
                case "lock": Box(26,20,70,53);Line(33,53,33,71);Line(33,71,42,80);Line(42,80,54,80);Line(54,80,63,71);Line(63,71,63,53);Circle(48,37,3);break;
                case "fan": Circle(48,48,7);for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Circle(48+19*Mathf.Cos(a),48+19*Mathf.Sin(a),12);}break;
                case "vacuum": Circle(48,47,29);Line(24,30,72,30);Circle(48,57,7);break;
                case "person": case "motion": Circle(48,68,10);Line(48,58,48,32);Line(48,48,28,38);Line(48,48,68,38);Line(48,32,32,16);Line(48,32,64,16);break;
                case "sun": Circle(48,48,17);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(48+26*Mathf.Cos(a),48+26*Mathf.Sin(a),48+34*Mathf.Cos(a),48+34*Mathf.Sin(a));}break;
                case "alarm": Line(48,80,17,23);Line(17,23,79,23);Line(79,23,48,80);Line(48,37,48,57,3);Circle(48,30,2);break;
                case "search": Circle(41,55,22);Line(57,38,77,18,3);break;
                case "settings": Circle(48,48,13);Circle(48,48,27);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(48+26*Mathf.Cos(a),48+26*Mathf.Sin(a),48+35*Mathf.Cos(a),48+35*Mathf.Sin(a),4);}break;
                case "map": Box(20,21,76,75);Line(48,21,48,50);Line(20,50,76,50);Line(48,62,48,75);break;
                case "automation": Circle(25,48,7);Circle(70,70,7);Circle(70,26,7);Line(32,48,50,48);Line(50,48,50,70);Line(50,70,63,70);Line(50,48,50,26);Line(50,26,63,26);break;
                default: Circle(48,48,29);Line(27,42,39,54);Line(39,54,49,37);Line(49,37,69,60);break;
            }
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false); texture.SetPixels(pixels);texture.Apply(false,true);
            result = Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*0.5f,100);cache[name]=result;return result;
        }
    }
}
