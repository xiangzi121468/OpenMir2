// 配饰装备图标加载代码片段
// 添加到 MShare.pas

const
  ACCESSORYIMAGESFILE = 'Data\AccessoryItems.data';

var
  g_WAccessoryImages: TWMImages;

// 在初始化时加载
procedure InitAccessoryImages;
begin
  g_WAccessoryImages := TWMImages.Create;
  g_WAccessoryImages.FileName := ACCESSORYIMAGESFILE;
  g_WAccessoryImages.Initialize;
end;

// 获取配饰图标
function GetAccessoryImage(nIndex: Integer): TDirectDrawSurface;
begin
  if nIndex < g_WAccessoryImages.ImageCount then
    Result := g_WAccessoryImages.Images[nIndex]
  else
    Result := nil;
end;

// 配饰索引常量
const
  // 戒指 (Ring)
  IDX_RING_ICE_CRYSTAL = 50;    // 冰晶戒指
  IDX_RING_FROST_SOUL = 51;     // 霜魄戒指
  IDX_RING_FROST_CONDENSE = 52; // 凝霜戒指
  IDX_RING_NIGHT_WALK = 53;     // 夜行戒指
  IDX_RING_SHADOW = 54;         // 暗影戒指
  IDX_RING_UNDEAD = 55;         // 亡灵戒指
  IDX_RING_SOUL_DEVOUR = 56;    // 噬魂戒指
  IDX_RING_SKELETON = 57;       // 骸骨戒指
  IDX_RING_BLOOD_SOUL = 58;     // 血魄戒指
  IDX_RING_BLOOD_BAT = 59;      // 血蝠戒指
  IDX_RING_DRAGON_FLAME = 60;   // 龙炎戒指
  IDX_RING_DRAGON_SOUL = 61;    // 龙魂戒指
  IDX_RING_DRAGON_MIGHT = 62;   // 龙威戒指
  IDX_RING_SLIME = 63;          // 黏液戒指
  
  // 靴子 (Boots)
  IDX_BOOTS_ICE = 200;          // 寒冰之靴
  IDX_BOOTS_FROST_SOUL = 201;   // 冰魄之靴
  IDX_BOOTS_FROST = 202;        // 凝霜之靴
  IDX_BOOTS_DARK_NIGHT = 203;   // 暗夜之靴
  IDX_BOOTS_UNDEAD = 204;       // 亡灵之靴
  IDX_BOOTS_SOUL_DEVOUR = 205;  // 噬魂之靴
  IDX_BOOTS_SKELETON = 206;     // 骸骨之靴
  IDX_BOOTS_BLOOD_SHADOW = 207; // 血影之靴
  IDX_BOOTS_DRAGON_FLAME = 208; // 龙炎之靴
  IDX_BOOTS_DRAGON_SOUL = 209;  // 龙魂之靴
  IDX_BOOTS_DRAGON_MIGHT = 210; // 龙威之靴
  IDX_BOOTS_SLIME = 211;        // 黏液之靴
  
  // 腰带 (Belt)
  IDX_BELT_ICE = 250;           // 寒冰腰带
  IDX_BELT_FROST_SOUL = 251;    // 冰魄腰带
  IDX_BELT_FROST = 252;         // 凝霜腰带
  IDX_BELT_SHADOW = 253;        // 暗影腰带
  IDX_BELT_UNDEAD = 254;        // 亡灵腰带
  IDX_BELT_SOUL_DEVOUR = 255;   // 噬魂腰带
  IDX_BELT_SKELETON = 256;      // 骸骨腰带
  IDX_BELT_BLOOD_BAT = 257;     // 血蝠腰带
  IDX_BELT_DRAGON_FLAME = 258;  // 龙炎腰带
  IDX_BELT_DRAGON_SOUL = 259;   // 龙魂腰带
  IDX_BELT_DRAGON_MIGHT = 260;  // 龙威腰带
  IDX_BELT_SLIME = 261;         // 黏液腰带
