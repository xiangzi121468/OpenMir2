//******************************************************************************
//*                                                                            *
//*                         AsphyreSphinx??????                              *
//*                         ????????? QQ117594672                             *
//*                                                                            *
//******************************************************************************
unit MShare;

interface

uses
  Windows, Classes, Dialogs, SysUtils, Forms, PXL.Canvas, PXL.Textures, PXL.Providers, pngimage,
  PXL.SwapChains, PXL.Types, PXL.Devices, DWinCtl, HumanActor, Actor, Grobal2, Wil, uEDcode,
  DIB, GList, IniFiles, HashList, Graphics, MirEffect, CnHashTable, uCommon, uGameEngine,SimpleMsgPack;

const
  MAXX = 52; //SCREENWIDTH div 20;
  MAXY = 40; //SCREENWIDTH div 20;
  LONGHEIGHT_IMAGE = 35;
  FLASHBASE = 410;
  SOFFX = 0;
  SOFFY = 0;
  HEALTHBAR_BLACK = 0;
  HEALTHBAR_RED = 1;
  BARWIDTH = 30;
  BARHEIGHT = 2;
  MAXSYSLINE = 8;
  BOTTOMBOARD = 1;
  AREASTATEICONBASE = 150;
  g_WinBottomRetry = 45;
  ResourceDir: String = '91Resource\';
  UserCfgDir: String = '91Resource\Users\';
  DEFSCREENWIDTH = 800;
  DEFSCREENHEIGHT = 600;
var
  g_UIInitialized: Boolean = False;
  g_Titles: THumTitles;
  g_ActiveTitle: THumTitle;
  g_hTitles: THumTitles;
  g_hActiveTitle: THumTitle;

  g_fWZLFirst: Byte = 7;

  g_boLogon: Boolean = False;
  g_fGoAttack: Boolean = False;

  g_dwReGetMapTitleTick: Longword = 0;
  g_QueryWinBottomTick: Longword;
  g_fGetRenderBottom: Boolean = False;

  g_nDragonRageStateIndex: Integer = 0;
  SCREENWIDTH: Integer = 800;
  SCREENHEIGHT: Integer = 600;
  g_VSync: Boolean = True;
  AAX: Integer = 26 + 14;
  LMX: Integer = 30;
  LMY: Integer = 26;
  VIEWWIDTH: Integer = 8;

  BOTTOMTOP: Integer = 800 - 251;
  WINRIGHT: Integer = 800 - 60;
  BOTTOMEDGE: Integer = 600 - 30;
  SURFACEMEMLEN: Integer = ((800 + 3) div 4) * 4 * 600;
  MAPSURFACEWIDTH: Integer = 800;
  MAPSURFACEHEIGHT: Integer = 600 - 150;
  BOXWIDTH: Integer = (800 div 2 - 214) * 2;

  g_SkidAD_Rect: TRect;
  g_SkidAD_Rect2: TRect;
  G_RC_LEVEL: TRect;
  G_RC_EXP: TRect;
  G_RC_WEIGTH: TRect;
  G_RC_SQUENGINER: TRect;
  G_RC_IMEMODE: TRect;

const
  {------------}
  NEWHINTSYS = True;

  NPC_CILCK_INVTIME = 500;
  MAXITEMBOX_WIDTH = 177;
  MAXMAGICLV = 3;
  RUNLOGINCODE = 1200205220;
  CLIENT_VERSION_NUMBER = 120020522;
  STDCLIENT = 0;
  RMCLIENT = 46;
  CLIENTTYPE = RMCLIENT;
  CUSTOMLIBFILE = 0;
  DEBUG = 0;
  LOGICALMAPUNIT = 30; //1108 40;
  HUMWINEFFECTTICK = 200;

  WINLEFT = 60;
  WINTOP = 60;
  MINIMAPSIZE = 200;

  MAPDIR = '.\Map\'; //????????????
  CONFIGFILE = '.\Config\%s.ini';
{$IF CUSTOMLIBFILE = 1}
  EFFECTIMAGEDIR = 'Graphics\Effect\';
  MAINIMAGEFILE = 'Graphics\FrmMain\Main.data';
  MAINIMAGEFILE2 = 'Graphics\FrmMain\Main2.data';
  MAINIMAGEFILE3 = 'Graphics\FrmMain\Main3.data';
{$ELSE}
  MAINIMAGEFILE = 'Data\Prguse.data';
  MAINIMAGEFILE2 = 'Data\Prguse2.data';
  MAINIMAGEFILE3 = 'Data\Prguse3.data';
  MAINIMAGEFILE3_16 = 'Data\Prguse3_16.data';
  EFFECTIMAGEDIR = 'Data\';
{$IFEND}

  CHRSELIMAGEFILE = 'Data\ChrSel.data';
  MINMAPIMAGEFILE = 'Data\mmap.data';
  TITLESIMAGEFILE = 'Data\Tiles.data';
  TITLESIMAGEFILEX = 'Data\Tiles%d.data';

  SMLTITLESIMAGEFILE = 'Data\SmTiles.data';
  SMLTITLESIMAGEFILEX = 'Data\SmTiles%d.data';


  HUMWINGIMAGESFILE = 'Data\HumEffect.data';
  HUMWINGIMAGESFILE2 = 'Data\HumEffect2.data';
  HUMWINGIMAGESFILE3 = 'Data\HumEffect3.data';
  MAGICONIMAGESFILE = 'Data\MagIcon.data';
  MAGICON2IMAGESFILE = 'Data\MagIcon2.data';

  HUMIMGIMAGESFILE = 'Data\Hum.data';
  HUM2IMGIMAGESFILE = 'Data\Hum2.data';
  HUM3IMGIMAGESFILE = 'Data\Hum3.data';
  HUM4IMGIMAGESFILE = 'Data\NewHum.data';  // 新增衣服资源 (Shape 50-63)
  ARMOREFFECTFILE = 'Data\ArmorEffect.data';  // 衣服装备特效

  HAIRIMGIMAGESFILE = 'Data\Hair.data';
  HAIR2IMGIMAGESFILE = 'Data\Hair2.data';

  WEAPONIMAGESFILE = 'Data\Weapon.data';
  WEAPON2IMAGESFILE = 'Data\Weapon2.data';
  WEAPON3IMAGESFILE = 'Data\Weapon3.data';

  NPCIMAGESFILE = 'Data\Npc.data';
  NPC2IMAGESFILE = 'Data\Npc2.data';
  MAGICIMAGESFILE = 'Data\Magic.data';
  MAGIC2IMAGESFILE = 'Data\Magic2.data';
  MAGIC3IMAGESFILE = 'Data\Magic3.data';
  MAGIC4IMAGESFILE = 'Data\Magic4.data';

  MAGIC5IMAGESFILE = 'Data\Magic5.data';
  MAGIC6IMAGESFILE = 'Data\Magic6.data';
  MAGIC7IMAGESFILE = 'Data\magic7-16.data';
  MAGIC7IMAGESFILE2 = 'Data\magic7.data';
  MAGIC8IMAGESFILE = 'Data\magic8-16.data';
  MAGIC8IMAGESFILE2 = 'Data\magic8.data';
  MAGIC9IMAGESFILE = 'Data\magic9.data';
  MAGIC10IMAGESFILE = 'Data\magic10.data';
  STATEEFFECTFILE = 'Data\StateEffect.data';

  SHINEEFFECTFILE = 'Data\ShineEffect.data';
 // SHINEITEMFILE = 'Data\Shineitem.data'; //????
  SHINEITEMSFILE = 'Data\ShineItems.data'; //????
  SHINEDNITEMSFILE = 'Data\ShineDnItems.data'; //????

  ANITILESIMAGEFILE = 'Data\AniTiles1.data';

  EVENTEFFECTIMAGESFILE = EFFECTIMAGEDIR + 'Event.data';

  BAGITEMIMAGESFILE = 'Data\Items.data';
  BAGITEMIMAGESFILE2 = 'Data\Items2.data';
  STATEITEMIMAGESFILE = 'Data\StateItem.data';
  STATEITEMIMAGESFILE2 = 'Data\StateItem2.data';
  DNITEMIMAGESFILE = 'Data\DnItems.data';
  DNITEMIMAGESFILE2 = 'Data\DnItems2.data';
  ACCESSORYIMAGESFILE = 'Data\AccessoryItems.data';  // 配饰装备图标 (戒指、靴子、腰带)

  OBJECTIMAGEFILE = 'Data\Objects.data';
  OBJECTIMAGEFILE1 = 'Data\Objects%d.data';
  MONIMAGEFILE = 'Data\Mon%d.data';
  DRAGONIMAGEFILE = 'Data\Dragon.data';
  EFFECTIMAGEFILE = 'Data\Effect.data';

  cboEffectFile = 'Data\cboEffect.data';
  cbohairFile = 'Data\cbohair.data';
  cbohumFile = 'Data\cbohum.data';
  cbohum3File = 'Data\cbohum3.data';

  cboHumEffectFile = 'Data\cboHumEffect.data';
  cboHumEffect2File = 'Data\cboHumEffect2.data';
  cboHumEffect3File = 'Data\cboHumEffect3.data';
  cboweaponFile = 'Data\cboweapon.data';
  cboweapon3File = 'Data\cboweapon3.data';

  bInterfaceFile = 'Data\bInterface.data';

  MONIMAGEFILEEX = 'Data\%d.data';
  MONPMFILE = 'Data\%d.pm';

  //MAXX                      = SCREENWIDTH div 20;
  //MAXY                      = SCREENWIDTH div 20;

  DEFAULTCURSOR = 0; //???????
  IMAGECURSOR = 1; //?????
  USECURSOR = DEFAULTCURSOR; //????????????

  MAXBAGITEMCL = 52;
  MAXFONT = 8;
  ENEMYCOLOR = 69;
  HERO_MIIDX_OFFSET = 5000;
  SAVE_MIIDX_OFFSET = HERO_MIIDX_OFFSET + 500;
  STALL_MIIDX_OFFSET = HERO_MIIDX_OFFSET + 500 + 50;
  DETECT_MIIDX_OFFSET = HERO_MIIDX_OFFSET + 500 + 50 + 10 + 1;
  MSGMUCH = 2;

  g_sHumAttr: array[1..5] of string = ('??', '?', '?', '??', '??');
{$IFDEF UI_0508}
  g_DBStateStrArr: array[0..3] of string = ('???', '??', '????', '????');
  g_DBStateStrArrUS: array[0..2] of string = ('???', '??', '???');
  g_DBStateStrArr2: array[0..3] of string = ('??', '????', '????', '????');
{$ELSE}
  g_DBStateStrArr: array[0..6] of string = ('???', '??', '??', '????', '???', '????', '????');
  g_DBStateStrArrUS: array[0..2] of string = ('???', '??', '???');
  g_DBStateStrArr2: array[0..4] of string = ('??', '????', '????', '????', '????');
{$ENDIF}
{$IF SERIESSKILL}
  g_VenationLvStrArr: array[0..6] of string = ('??????', '???????', '??????', '???????', '???????', '???????', '???????');
{$IFEND SERIESSKILL}

  g_slegend: array[0..13] of string = (
    '', //0
    '??????', //1
    '???????', //2
    '????????', //3
    '???????', //4
    '', //5
    '??????', //6
    '', //7
    '???????', //8
    '', //9
    '????????', //10
    '??????', //11
    '', //12
    '???????' //13
    );

  //config
  MAX_GC_GENERAL = 16;
  g_ptGeneral: array[0..MAX_GC_GENERAL] of TRect = (
    (Left: 35 + 000; Top: 70 + 23 * 0; Right: 35 + 000 + 72 + 18; Bottom: 70 + 23 * 0 + 16), //0
    (Left: 35 + 000; Top: 70 + 23 * 1; Right: 35 + 000 + 72 + 18; Bottom: 70 + 23 * 1 + 16), //1
    (Left: 35 + 000; Top: 70 + 23 * 2; Right: 35 + 000 + 78 + 18; Bottom: 70 + 23 * 2 + 16), //2
    (Left: 35 + 000; Top: 70 + 23 * 3; Right: 35 + 000 + 96; Bottom: 70 + 23 * 3 + 16), //3

    (Left: 35 + 120; Top: 70 + 23 * 0; Right: 35 + 120 + 72 + 30; Bottom: 70 + 23 * 0 + 16), //4
    (Left: 35 + 120; Top: 70 + 23 * 1; Right: 35 + 120 + 72; Bottom: 70 + 23 * 1 + 16), //5
    (Left: 35 + 120; Top: 70 + 23 * 2; Right: 35 + 120 + 72 + 18; Bottom: 70 + 23 * 2 + 16), //6
    (Left: 35 + 120; Top: 70 + 23 * 3; Right: 35 + 120 + 72; Bottom: 70 + 23 * 3 + 16), //7
    (Left: 35 + 120; Top: 70 + 23 * 4; Right: 35 + 120 + 72 + 18; Bottom: 70 + 23 * 4 + 16),

    (Left: 35 + 240; Top: 70 + 23 * 0; Right: 35 + 240 + 72; Bottom: 70 + 23 * 0 + 16),
    (Left: 35 + 240; Top: 70 + 23 * 1; Right: 35 + 240 + 72; Bottom: 70 + 23 * 1 + 16),
    (Left: 35 + 240; Top: 70 + 23 * 2; Right: 35 + 240 + 48; Bottom: 70 + 23 * 2 + 16),
    (Left: 35 + 240; Top: 70 + 23 * 3; Right: 35 + 240 + 72; Bottom: 70 + 23 * 3 + 16),
    (Left: 35 + 240; Top: 70 + 23 * 4; Right: 35 + 240 + 72; Bottom: 70 + 23 * 4 + 16),
    (Left: 35 + 240; Top: 70 + 23 * 5; Right: 35 + 240 + 72; Bottom: 70 + 23 * 5 + 16),

    (Left: 35 + 120; Top: 70 + 23 * 5; Right: 35 + 120 + 72; Bottom: 70 + 23 * 5 + 16),
    (Left: 35 + 000; Top: 70 + 23 * 5; Right: 35 + 000 + 96; Bottom: 70 + 23 * 5 + 16)
    );
  g_caGeneral: array[0..MAX_GC_GENERAL] of string = (
    '???????(Z)', //0
    '??????(X)', //1
    '??Shift??(C)', //2
    '??????????', //3
    '??????(ESC)', //4
    '???????', //'???????',                     //5
    '??????????', //6
    '??????', //'???????',                     //7
    '???????(V)', //8
    '???????', //9
    '?????', //10
    '???',     //11
    '??????',  //12
    '??????',   //13
    '??????',   //14
    '????????',    //15
    '??????'      //16
    );
  g_HintGeneral: array[0..MAX_GC_GENERAL] of string = (
    '????????????????????', //0
    '????????????????????????', //1
    '??????????????Shift???\???????????', //2
    '??????????????????????\????????????', //3
    '????????????????????\?????????', //4
    '?????????????????????????', //5
    '????????????????????????????\???????????????????????????', //6
    '', //'???????????????????\???????????', //7
    '??????????????????????\????????????????????????', //8
    '?????????????????????\????????????????????????', //9
    '?????????????????????????',
    '????????????????????\????????????',
    '',
    '??????????????????????',
    '??????????????',
    '??????????????????',
    '?????????????????????'
    );
  g_gcGeneral: array[0..MAX_GC_GENERAL] of Boolean = (True, True, False, True, True, True, False, False, False, True, True, True, False, True, False, True, True);
  g_clGeneral: array[0..MAX_GC_GENERAL] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver);
  g_clGeneralDef: array[0..MAX_GC_GENERAL] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver);

  //====================================Protect====================================
  MAX_GC_PROTECT = 11;
  g_ptProtect: array[0..MAX_GC_PROTECT] of TRect = (
    (Left: 35 + 000; Top: 70 + 24 * 0; Right: 35 + 000 + 20; Bottom: 70 + 24 * 0 + 16), //0
    (Left: 35 + 000; Top: 70 + 24 * 1; Right: 35 + 000 + 20; Bottom: 70 + 24 * 1 + 16), //1
    (Left: 35 + 000; Top: 70 + 24 * 2; Right: 35 + 000 + 20; Bottom: 70 + 24 * 2 + 16), //2
    (Left: 35 + 000; Top: 70 + 24 * 3; Right: 35 + 000 + 20; Bottom: 70 + 24 * 3 + 16), //3
    (Left: 35 + 000; Top: 70 + 24 * 4; Right: 35 + 000 + 20; Bottom: 70 + 24 * 4 + 16), //4
    (Left: 35 + 000; Top: 70 + 24 * 5; Right: 35 + 000 + 20; Bottom: 70 + 24 * 5 + 16), //5
    (Left: 35 + 000; Top: 70 + 24 * 6; Right: 35 + 000 + 72; Bottom: 70 + 24 * 6 + 16), //6
    (Left: 35 + 180; Top: 70 + 24 * 0; Right: 35 + 180 + 20; Bottom: 70 + 24 * 0 + 16), //0
    (Left: 35 + 180; Top: 70 + 24 * 1; Right: 35 + 180 + 20; Bottom: 70 + 24 * 1 + 16), //1
    (Left: 35 + 180; Top: 70 + 24 * 3; Right: 35 + 180 + 20; Bottom: 70 + 24 * 3 + 16),
    (Left: 35 + 180; Top: 70 + 24 * 5; Right: 35 + 180 + 20; Bottom: 70 + 24 * 5 + 16),
    (Left: 35 + 180; Top: 70 + 24 * 6; Right: 35 + 180 + 20; Bottom: 70 + 24 * 6 + 16)
    );
  g_caProtect: array[0..MAX_GC_PROTECT] of string = (
    'HP               ????', //0
    'MP               ????', //1
    '', //2
    'HP               ????', //3
    '', //4
    'HP               ????', //5
    '????????', //6
    'HP               ????', //7
    'MP               ????', //8
    'HP               ????', //9
    'HP', //10
    'MP?????????????????'
    );
  g_sRenewBooks: array[0..MAX_GC_PROTECT] of string = (
    '????????', //shape = 2
    '?????????', //shape = 1
    '????', //shape = 3
    '??????', //shape = 5
    '????????',
    '???????',
    '????????',
    '',
    '',
    '',
    '',
    ''
    );
  g_gcProtect: array[0..MAX_GC_PROTECT] of Boolean = (False, False, False, False, False, False, False, True, True, True, False, True);
  g_gnProtectPercent: array[0..MAX_GC_PROTECT] of Integer = (10, 10, 10, 10, 10, 10, 0, 88, 88, 88, 20, 00);
  g_gnProtectTime: array[0..MAX_GC_PROTECT] of Integer = (4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 4000, 1000, 1000, 1000);
  g_clProtect: array[0..MAX_GC_PROTECT] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clLime);

  //====================================Tec====================================
  MAX_GC_TEC = 14;
  g_ptTec: array[0..MAX_GC_TEC] of TRect = (
    (Left: 35 + 000; Top: 70 + 20 * 0; Right: 35 + 000 + 72; Bottom: 70 + 20 * 0 + 16), //0
    (Left: 35 + 000; Top: 70 + 20 * 1; Right: 35 + 000 + 72; Bottom: 70 + 20 * 1 + 16), //1
    (Left: 35 + 000; Top: 70 + 20 * 2; Right: 35 + 000 + 72; Bottom: 70 + 20 * 2 + 16), //2
    (Left: 35 + 000; Top: 70 + 20 * 3; Right: 35 + 000 + 72; Bottom: 70 + 20 * 3 + 16), //3

    (Left: 35 + 120; Top: 70 + 24 * 0; Right: 35 + 120 + 72; Bottom: 70 + 24 * 0 + 16), //4
    (Left: 35 + 120; Top: 70 + 24 * 1; Right: 35 + 120 + 72; Bottom: 70 + 24 * 1 + 16), //5

    (Left: 35 + 240; Top: 70 + 24 * 0; Right: 35 + 240 + 72; Bottom: 70 + 24 * 0 + 16), //6

    (Left: 35 + 120; Top: 70 + 24 * 5; Right: 35 + 120 + 72; Bottom: 70 + 24 * 5 + 16), //7
    (Left: 35 + 120; Top: 70 + 24 * 6; Right: 35 + 120 + 20; Bottom: 70 + 24 * 6 + 16), //8

    (Left: 35 + 000; Top: 70 + 20 * 4; Right: 35 + 000 + 72; Bottom: 70 + 20 * 4 + 16), //9
    (Left: 35 + 000; Top: 70 + 20 * 5; Right: 35 + 000 + 72; Bottom: 70 + 20 * 5 + 16),
    (Left: 35 + 000; Top: 70 + 20 * 6; Right: 35 + 000 + 72; Bottom: 70 + 20 * 6 + 16),

    (Left: 35 + 240; Top: 70 + 24 * 5; Right: 35 + 240 + 72; Bottom: 70 + 24 * 5 + 16),
    (Left: 35 + 000; Top: 70 + 20 * 7; Right: 35 + 000 + 72; Bottom: 70 + 20 * 7 + 16),
    (Left: 35 + 120; Top: 70 + 24 * 2 + 12; Right: 35 + 120 + 72; Bottom: 70 + 24 * 2 + 16 + 12)
    );

  g_HintTec: array[0..MAX_GC_TEC] of string = (
    '??????????????????', //0
    '??????????????????', //1
    '???????????????????', //2
    '?????????????????????', //3
    '???????????????????', //4
    '???????????????????????', //5
    '???????????????????????', //6
    '',
    '',
    '??????????????????????', //7
    '???????????????????', //8
    '?????????????????', //9
    '???????????????????????\?????????????PK',
    '??????????????????',
    '????????????????????????????????????????????'
    );
  g_caTec: array[0..MAX_GC_TEC] of string = (
    '???????', //0
    '???????', //1
    '??????', //2
    '???????', //3
    '???????', //4
    '????????(???)', //5
    '???????', //6
    '?????', //7
    '', //8
    '???????', //9
    '??????',
    '???????',
    '????????????',
    '????????',
    '??????????????');
  g_sMagics: array[0..MAX_GC_TEC] of string = (
    '??????',
    '??????',
    '?????',
    '?????',
    '???????',
    '?????',
    '??????',
    '??????',
    '?????',
    '?????',
    '?????',
    '?????',
    '?????',
    '?????',
    '?????');
  g_gnTecPracticeKey: Integer = 0;
  g_gcTec: array[0..MAX_GC_TEC] of Boolean = (True, True, True, True, True, True, False, False, False, False, False, False, False, True, False);
  g_gnTecTime: array[0..MAX_GC_TEC] of Integer = (0, 0, 0, 0, 0, 0, 0, 0, 4000, 0, 0, 0, 0, 0, 0);
  g_clTec: array[0..MAX_GC_TEC] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver);

  //====================================Assistant====================================
  MAX_GC_ASS = 6;
  g_ptAss: array[0..MAX_GC_ASS] of TRect = (
    (Left: 35 + 000; Top: 70 + 24 * 0; Right: 35 + 000 + 142; Bottom: 70 + 24 * 0 + 16), //0
    (Left: 35 + 000; Top: 70 + 24 * 1; Right: 35 + 000 + 72; Bottom: 70 + 24 * 1 + 16), //1
    (Left: 35 + 000; Top: 70 + 24 * 2; Right: 35 + 000 + 72; Bottom: 70 + 24 * 2 + 16), //2
    (Left: 35 + 000; Top: 70 + 24 * 3; Right: 35 + 000 + 72; Bottom: 70 + 24 * 3 + 16), //3
    (Left: 35 + 000; Top: 70 + 24 * 4; Right: 35 + 000 + 72; Bottom: 70 + 24 * 4 + 16), //4
    (Left: 35 + 000; Top: 70 + 24 * 5; Right: 35 + 000 + 120; Bottom: 70 + 24 * 5 + 16), //5
    (Left: 35 + 000; Top: 70 + 24 * 6; Right: 35 + 000 + 120; Bottom: 70 + 24 * 6 + 16)
    );
  g_HintAss: array[0..MAX_GC_ASS] of string = (
    '',
    '', //0
    '', //1
    '', //2
    '', //3
    '????????????????????????????\???????????? [???] ?????????', //4
    '');
  g_caAss: array[0..MAX_GC_ASS] of string = (
    '???????(Ctrl+Alt+X)',
    '?????????', //0
    '?????????', //1
    '??????????', //2
    '??????????', //3
    '??????????(?????)', //4
    '?????????(?????)');
  g_gcAss: array[0..MAX_GC_ASS] of Boolean = (False, False, False, False, False, False, False);
  g_clAss: array[0..MAX_GC_ASS] of TColor = (clLime, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver);

  //====================================HotKey====================================
  MAX_GC_HOTKEY = 8;
  g_ptHotkey: array[0..MAX_GC_HOTKEY] of TRect = (
    (Left: 25 + 000; Top: 43 + 20 * 0; Right: 35 + 000 + 99; Bottom: 43 + 20 * 0 + 16), //0
    (Left: 25 + 000; Top: 43 + 20 * 2; Right: 35 + 000 + 80; Bottom: 43 + 20 * 2 + 16), //2
    (Left: 25 + 000; Top: 43 + 20 * 3; Right: 35 + 000 + 80; Bottom: 43 + 20 * 3 + 16), //3
    (Left: 25 + 000; Top: 43 + 20 * 4; Right: 35 + 000 + 80; Bottom: 43 + 20 * 4 + 16), //4
    (Left: 25 + 000; Top: 43 + 20 * 5; Right: 35 + 000 + 80; Bottom: 43 + 20 * 5 + 16), //5
    (Left: 25 + 000; Top: 43 + 20 * 6; Right: 35 + 000 + 80; Bottom: 43 + 20 * 6 + 16), //6
    (Left: 25 + 000; Top: 43 + 20 * 7; Right: 35 + 000 + 80; Bottom: 43 + 20 * 7 + 16), //7
    (Left: 25 + 000; Top: 43 + 20 * 8; Right: 35 + 000 + 80; Bottom: 43 + 20 * 8 + 16),
    (Left: 25 + 000; Top: 43 + 20 * 9; Right: 35 + 000 + 80; Bottom: 43 + 20 * 9 + 16) //8
    );
  g_caHotkey: array[0..MAX_GC_HOTKEY] of string = (
    '???????????',
    '??????', //0
    '?????????', //1
    '?????????', //2
    '????????', //3
    '????????', //4
    '?????????', //5
    '???????',
    '???????'
    );
  g_gcHotkey: array[0..MAX_GC_HOTKEY] of Boolean = (False, False, False, False, False, False, False, False, False);
  g_gnHotkey: array[0..MAX_GC_HOTKEY] of Integer = (0, 0, 0, 0, 0, 0, 0, 0, 0);
  g_clHotkey: array[0..MAX_GC_HOTKEY] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clLime);

  //====================================???====================================
var
  g_BuildBotTex: Byte = 0;
  g_WinBottomType: Byte = 0;
//  g_Windowed: Boolean = False;
  g_pkeywords: PString = nil;

  g_boAutoPickUp: Boolean = True;
  g_boPickUpAll: Boolean = False;
  g_ptItems_Pos: Integer = -1;
  g_ptItems_Type: Integer = 0;
  g_ItemsFilter_All: TCnHashTableSmall;
  g_ItemsFilter_All_Def: TCnHashTableSmall;
  g_ItemsFilter_Dress: TStringList;
  g_ItemsFilter_Weapon: TStringList;
  g_ItemsFilter_Headgear: TStringList;
  g_ItemsFilter_Drug: TStringList;
  g_ItemsFilter_Other: TStringList;

  g_xMapDescList: TStringList;
  g_xCurMapDescList: TStringList;

const

  MAX_GC_ITEMS = 7;
  g_ptItemsA: TRect = (Left: 25 + 194; Top: 68 + 18 * 7 + 23; Right: 25 + 194 + 80; Bottom: 68 + 18 * 7 + 16 + 23);
  g_ptAutoPickUp: TRect = (Left: 25 + 267; Top: 68 + 18 * 7 + 23; Right: 25 + 267 + 80; Bottom: 68 + 18 * 7 + 16 + 23);

  g_ptItems0: array[0..MAX_GC_ITEMS] of TRect = (
    (Left: 25 + 000; Top: 68 + 18 * 0; Right: 25 + 000 + 120; Bottom: 68 + 18 * 0 + 16), //0
    (Left: 25 + 000; Top: 68 + 18 * 1; Right: 25 + 000 + 120; Bottom: 68 + 18 * 1 + 16), //1
    (Left: 25 + 000; Top: 68 + 18 * 2; Right: 25 + 000 + 120; Bottom: 68 + 18 * 2 + 16), //2
    (Left: 25 + 000; Top: 68 + 18 * 3; Right: 25 + 000 + 120; Bottom: 68 + 18 * 3 + 16), //3
    (Left: 25 + 000; Top: 68 + 18 * 4; Right: 25 + 000 + 120; Bottom: 68 + 18 * 4 + 16), //4
    (Left: 25 + 000; Top: 68 + 18 * 5; Right: 25 + 000 + 120; Bottom: 68 + 18 * 5 + 16), //5
    (Left: 25 + 000; Top: 68 + 18 * 6; Right: 25 + 000 + 120; Bottom: 68 + 18 * 6 + 16),
    (Left: 25 + 000; Top: 68 + 18 * 7; Right: 25 + 000 + 120; Bottom: 68 + 18 * 7 + 16)
    );
  g_ptItems1: array[0..MAX_GC_ITEMS] of TRect = (
    (Left: 25 + 160; Top: 68 + 18 * 0; Right: 25 + 160 + 16; Bottom: 68 + 18 * 0 + 16), //0
    (Left: 25 + 160; Top: 68 + 18 * 1; Right: 25 + 160 + 16; Bottom: 68 + 18 * 1 + 16), //1
    (Left: 25 + 160; Top: 68 + 18 * 2; Right: 25 + 160 + 16; Bottom: 68 + 18 * 2 + 16), //2
    (Left: 25 + 160; Top: 68 + 18 * 3; Right: 25 + 160 + 16; Bottom: 68 + 18 * 3 + 16), //3
    (Left: 25 + 160; Top: 68 + 18 * 4; Right: 25 + 160 + 16; Bottom: 68 + 18 * 4 + 16), //4
    (Left: 25 + 160; Top: 68 + 18 * 5; Right: 25 + 160 + 16; Bottom: 68 + 18 * 5 + 16), //5
    (Left: 25 + 160; Top: 68 + 18 * 6; Right: 25 + 160 + 16; Bottom: 68 + 18 * 6 + 16),
    (Left: 25 + 160; Top: 68 + 18 * 7; Right: 25 + 160 + 16; Bottom: 68 + 18 * 7 + 16)
    );
  g_ptItems2: array[0..MAX_GC_ITEMS] of TRect = (
    (Left: 25 + 220; Top: 68 + 18 * 0; Right: 25 + 220 + 16; Bottom: 68 + 18 * 0 + 16), //0
    (Left: 25 + 220; Top: 68 + 18 * 1; Right: 25 + 220 + 16; Bottom: 68 + 18 * 1 + 16), //1
    (Left: 25 + 220; Top: 68 + 18 * 2; Right: 25 + 220 + 16; Bottom: 68 + 18 * 2 + 16), //2
    (Left: 25 + 220; Top: 68 + 18 * 3; Right: 25 + 220 + 16; Bottom: 68 + 18 * 3 + 16), //3
    (Left: 25 + 220; Top: 68 + 18 * 4; Right: 25 + 220 + 16; Bottom: 68 + 18 * 4 + 16), //4
    (Left: 25 + 220; Top: 68 + 18 * 5; Right: 25 + 220 + 16; Bottom: 68 + 18 * 5 + 16), //5
    (Left: 25 + 220; Top: 68 + 18 * 6; Right: 25 + 220 + 16; Bottom: 68 + 18 * 6 + 16),
    (Left: 25 + 220; Top: 68 + 18 * 7; Right: 25 + 220 + 16; Bottom: 68 + 18 * 7 + 16)
    );
  g_ptItems3: array[0..MAX_GC_ITEMS] of TRect = (
    (Left: 25 + 280; Top: 68 + 18 * 0; Right: 25 + 280 + 16; Bottom: 68 + 18 * 0 + 16), //0
    (Left: 25 + 280; Top: 68 + 18 * 1; Right: 25 + 280 + 16; Bottom: 68 + 18 * 1 + 16), //1
    (Left: 25 + 280; Top: 68 + 18 * 2; Right: 25 + 280 + 16; Bottom: 68 + 18 * 2 + 16), //2
    (Left: 25 + 280; Top: 68 + 18 * 3; Right: 25 + 280 + 16; Bottom: 68 + 18 * 3 + 16), //3
    (Left: 25 + 280; Top: 68 + 18 * 4; Right: 25 + 280 + 16; Bottom: 68 + 18 * 4 + 16), //4
    (Left: 25 + 280; Top: 68 + 18 * 5; Right: 25 + 280 + 16; Bottom: 68 + 18 * 5 + 16), //5
    (Left: 25 + 280; Top: 68 + 18 * 6; Right: 25 + 280 + 16; Bottom: 68 + 18 * 6 + 16),
    (Left: 25 + 280; Top: 68 + 18 * 7; Right: 25 + 280 + 16; Bottom: 68 + 18 * 7 + 16)
    );

  g_caItems: array[0..MAX_GC_ITEMS] of pTCItemRule = (nil, nil, nil, nil, nil, nil, nil, nil);
  g_caItems2: array[0..MAX_GC_ITEMS] of pTCItemRule = (nil, nil, nil, nil, nil, nil, nil, nil);
  g_clItems: array[0..MAX_GC_ITEMS] of TColor = (clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver, clSilver);

  MAX_SERIESSKILL_POINT = 4;

  g_HitSpeedRate: Integer = 0;
  g_MagSpeedRate: Integer = 0;
  g_MoveSpeedRate: Integer = 0;

type
  TLoadMode = (lmUseWil, lmUseWzl, lmAutoWil);

  TMessageHandler = reference to procedure(AResult: Integer{TModalResult});
  TMessageDialogItem = record
    Text: String;
    Buttons: TMsgDlgButtons;
    Handler: TMessageHandler;
    Size: Integer;
  end;
  PTMessageDialogItem = ^TMessageDialogItem;

  IApplication = interface
    ['{501687F1-F936-4A5F-A235-34643269AF55}']
    procedure AddToChatBoardString(const Message: String; FColor, BColor: TColor);
    procedure LoadImage(const FileName: String; Index, Position: Integer);
//    procedure AddMessageDialog(const Text: String; Buttons: TMsgDlgButtons; Handler: TMessageHandler = nil; Size: Integer = 1);
    procedure Terminate;
    procedure DisConnect;
    function _CurPos: TPoint;
  end;

  TVaInfo = packed record
    cap: string;
    pt1: array[0..MAX_SERIESSKILL_POINT] of TRect;
    pt2: array[0..MAX_SERIESSKILL_POINT] of TRect;
    str1: array[0..MAX_SERIESSKILL_POINT] of string;
    Hint: array[0..MAX_SERIESSKILL_POINT] of string;
    //moved: array[0..MAX_SERIESSKILL_POINT] of Boolean;
    //downed: array[0..MAX_SERIESSKILL_POINT] of Boolean;
  end;

const
{$IF SERIESSKILL}
  g_VLastSender: TObject = nil;
  g_VMouseInfoTag: Integer = -1;
  g_VMouseInfo: string = '';

  g_hVLastSender: TObject = nil;
  g_hVMouseInfoTag: Integer = -1;
  g_hVMouseInfo: string = '';

  g_VLvHints: array[0..6] of string = (
    '????,????????????',
    '????,??????????,???\?????????????:%s',
    '???,???????+1,????????\??%s,???%s??????%d%s\???????%1.1n??',
    '????,???????????+1,????\%s??????%d%s,\???????%1.1n??',
    '????,???????????+1,???\????+2,????%s??????%d%s\???????%1.1n??',
    '????,???????????+1,???\????+5,???%s??????%d%s\???????%1.1n??',
    '????,???????????+1,???\????+9,??%s??????%d%s\???????%1.1n??'
    );

  g_VMouseInfo2: string = '';
  g_VLvHints2: string = '';

  g_hVMouseInfo2: string = '';
  g_hVLvHints2: string = '';

{$IFDEF UI_0508}
  g_VaInfos: array[0..3] of TVaInfo = (
    (
    pt1: (
    (Left: 35 + 038; Top: 30 + 000 + 50; Right: 35 + 038 + 23; Bottom: 30 + 000 + 50 + 23),
    (Left: 35 + 038; Top: 30 + 028 + 50; Right: 35 + 038 + 23; Bottom: 30 + 028 + 50 + 23),
    (Left: 37 + 038; Top: 30 + 053 + 50; Right: 37 + 038 + 23; Bottom: 30 + 053 + 50 + 23),
    (Left: 45 + 038; Top: 30 + 075 + 50; Right: 45 + 038 + 23; Bottom: 30 + 075 + 50 + 23),
    (Left: 50 + 038; Top: 30 + 057 + 50; Right: 50 + 038 + 23; Bottom: 30 + 057 + 50 + 23));
    pt2: (
    (Left: 35 + 047 + 038; Top: 30 + 005 + 50; Right: 35 + 047 + 28 + 038; Bottom: 30 + 005 + 12 + 50),
    (Left: 35 + 044 + 038; Top: 30 + 035 + 50; Right: 35 + 044 + 28 + 038; Bottom: 30 + 035 + 12 + 50),
    (Left: 37 - 034 + 038; Top: 30 + 058 + 50; Right: 37 - 034 + 28 + 038; Bottom: 30 + 058 + 12 + 50),
    (Left: 45 - 037 + 038; Top: 30 + 082 + 50; Right: 45 - 037 + 28 + 038; Bottom: 30 + 082 + 12 + 50),
    (Left: 50 + 038 + 038; Top: 30 + 063 + 50; Right: 50 + 038 + 28 + 038; Bottom: 30 + 063 + 12 + 50));
    str1: ('????', '???', '????', '????', '???');
    Hint: ('??????????+3', '??????????+3', '??????????+3', '??????????+3', '??????????+2');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 86; Top: 74; Right: 86 + 23; Bottom: 74 + 23),
    (Left: 85; Top: 113; Right: 35 + 038 + 23; Bottom: 30 + 028 + 50 + 23),
    (Left: 92; Top: 153; Right: 37 + 038 + 23; Bottom: 30 + 053 + 50 + 23),
    (Left: 99; Top: 192; Right: 45 + 038 + 23; Bottom: 30 + 075 + 50 + 23),
    (Left: 102; Top: 233; Right: 50 + 038 + 23; Bottom: 30 + 057 + 50 + 23));
    pt2: (
    (Left: 44; Top: 79; Right: 35 + 047 + 28 + 038; Bottom: 30 + 005 + 12 + 50),
    (Left: 43; Top: 118; Right: 35 + 044 + 28 + 038; Bottom: 30 + 035 + 12 + 50),
    (Left: 130; Top: 160; Right: 37 - 034 + 28 + 038; Bottom: 30 + 058 + 12 + 50),
    (Left: 62; Top: 198; Right: 45 - 037 + 28 + 038; Bottom: 30 + 082 + 12 + 50),
    (Left: 68; Top: 238; Right: 50 + 038 + 28 + 038; Bottom: 30 + 063 + 12 + 50));
    str1: ('????', '???', '????', '???', '???');
    Hint: ('??????????+2', '??????????+2', '??????????+2', '??????????+1', '??????????+1');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 84; Top: 86; Right: 35 + 038 + 23; Bottom: 30 + 000 + 50 + 23),
    (Left: 78; Top: 117; Right: 35 + 038 + 23; Bottom: 30 + 028 + 50 + 23),
    (Left: 77; Top: 155; Right: 37 + 038 + 23; Bottom: 30 + 053 + 50 + 23),
    (Left: 73; Top: 188; Right: 45 + 038 + 23; Bottom: 30 + 075 + 50 + 23),
    (Left: 70; Top: 224; Right: 50 + 038 + 23; Bottom: 30 + 057 + 50 + 23));
    pt2: (
    (Left: 42; Top: 91; Right: 35 + 047 + 28 + 038; Bottom: 30 + 005 + 12 + 50),
    (Left: 124; Top: 123; Right: 35 + 044 + 28 + 038; Bottom: 30 + 035 + 12 + 50),
    (Left: 41; Top: 160; Right: 37 - 034 + 28 + 038; Bottom: 30 + 058 + 12 + 50),
    (Left: 112; Top: 194; Right: 45 - 037 + 28 + 038; Bottom: 30 + 082 + 12 + 50),
    (Left: 107; Top: 229; Right: 50 + 038 + 28 + 038; Bottom: 30 + 063 + 12 + 50));
    str1: ('???', '????', '????', '????', '????');
    Hint: ('??????????+1', '??????????+1', '', '', '');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 86; Top: 70; Right: 35 + 038 + 23; Bottom: 30 + 000 + 50 + 23),
    (Left: 86; Top: 89; Right: 35 + 038 + 23; Bottom: 30 + 028 + 50 + 23),
    (Left: 86; Top: 110; Right: 37 + 038 + 23; Bottom: 30 + 053 + 50 + 23),
    (Left: 86; Top: 134; Right: 45 + 038 + 23; Bottom: 30 + 075 + 50 + 23),
    (Left: 86; Top: 158; Right: 50 + 038 + 23; Bottom: 30 + 057 + 50 + 23));
    pt2: (
    (Left: 42; Top: 74; Right: 35 + 047 + 28 + 038; Bottom: 30 + 005 + 12 + 50),
    (Left: 132; Top: 95; Right: 35 + 044 + 28 + 038; Bottom: 30 + 035 + 12 + 50),
    (Left: 42; Top: 116; Right: 37 - 034 + 28 + 038; Bottom: 30 + 058 + 12 + 50),
    (Left: 132; Top: 141; Right: 45 - 037 + 28 + 038; Bottom: 30 + 082 + 12 + 50),
    (Left: 42; Top: 164; Right: 50 + 038 + 28 + 038; Bottom: 30 + 063 + 12 + 50));
    str1: ('???', '???', '??', '????', '????');
    Hint: ('', '', '', '', '');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    )
    );
{$ELSE}
  g_VaInfos: array[0..3] of TVaInfo = (
    (
    pt1: (
    (Left: 35 + 038 + 52; Top: 30 + 000 + 50 + 113; Right: 35 + 038 + 52 + 23; Bottom: 30 + 000 + 50 + 113 + 23),
    (Left: 35 + 038 + 52; Top: 30 + 028 + 50 + 113; Right: 35 + 038 + 52 + 23; Bottom: 30 + 028 + 50 + 113 + 23),
    (Left: 37 + 038 + 52; Top: 30 + 053 + 50 + 113; Right: 37 + 038 + 52 + 23; Bottom: 30 + 053 + 50 + 113 + 23),
    (Left: 45 + 038 + 52; Top: 30 + 075 + 50 + 113; Right: 45 + 038 + 52 + 23; Bottom: 30 + 075 + 50 + 113 + 23),
    (Left: 50 + 038 + 52; Top: 30 + 057 + 50 + 113; Right: 50 + 038 + 52 + 23; Bottom: 30 + 057 + 50 + 113 + 23));
    pt2: (
    (Left: 35 + 047 + 52 + 038; Top: 30 + 005 + 113 + 50; Right: 35 + 047 + 28 + 52 + 038; Bottom: 30 + 005 + 12 + 113 + 50),
    (Left: 35 + 044 + 52 + 038; Top: 30 + 035 + 113 + 50; Right: 35 + 044 + 28 + 52 + 038; Bottom: 30 + 035 + 12 + 113 + 50),
    (Left: 37 - 034 + 52 + 038; Top: 30 + 058 + 113 + 50; Right: 37 - 034 + 28 + 52 + 038; Bottom: 30 + 058 + 12 + 113 + 50),
    (Left: 45 - 037 + 52 + 038; Top: 30 + 082 + 113 + 50; Right: 45 - 037 + 28 + 52 + 038; Bottom: 30 + 082 + 12 + 113 + 50),
    (Left: 50 + 038 + 52 + 038; Top: 30 + 063 + 113 + 50; Right: 50 + 038 + 28 + 52 + 038; Bottom: 30 + 063 + 12 + 113 + 50));
    str1: ('????', '???', '????', '????', '???');
    Hint: ('??????????+3', '??????????+3', '??????????+3', '??????????+3', '??????????+2');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 86 + 52; Top: 74 + 113; Right: 86 + 23 + 52; Bottom: 74 + 23 + 113),
    (Left: 85 + 52; Top: 113 + 113; Right: 35 + 038 + 23 + 52; Bottom: 30 + 028 + 50 + 23 + 113),
    (Left: 92 + 52; Top: 153 + 113; Right: 37 + 038 + 23 + 52; Bottom: 30 + 053 + 50 + 23 + 113),
    (Left: 99 + 52; Top: 192 + 113; Right: 45 + 038 + 23 + 52; Bottom: 30 + 075 + 50 + 23 + 113),
    (Left: 102 + 52; Top: 233 + 113; Right: 50 + 038 + 23 + 52; Bottom: 30 + 057 + 50 + 23 + 113));
    pt2: (
    (Left: 44 + 52; Top: 79 + 113; Right: 35 + 047 + 28 + 038 + 52; Bottom: 30 + 005 + 12 + 50 + 113),
    (Left: 43 + 52; Top: 118 + 113; Right: 35 + 044 + 28 + 038 + 52; Bottom: 30 + 035 + 12 + 50 + 113),
    (Left: 130 + 52; Top: 160 + 113; Right: 37 - 034 + 28 + 038 + 52; Bottom: 30 + 058 + 12 + 50 + 113),
    (Left: 62 + 52; Top: 198 + 113; Right: 45 - 037 + 28 + 038 + 52; Bottom: 30 + 082 + 12 + 50 + 113),
    (Left: 68 + 52; Top: 238 + 113; Right: 50 + 038 + 28 + 038 + 52; Bottom: 30 + 063 + 12 + 50 + 113));
    str1: ('????', '???', '????', '???', '???');
    Hint: ('??????????+2', '??????????+2', '??????????+2', '??????????+1', '??????????+1');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 84 + 52; Top: 86 + 113; Right: 35 + 038 + 23 + 52; Bottom: 30 + 000 + 50 + 23 + 113),
    (Left: 78 + 52; Top: 117 + 113; Right: 35 + 038 + 23 + 52; Bottom: 30 + 028 + 50 + 23 + 113),
    (Left: 77 + 52; Top: 155 + 113; Right: 37 + 038 + 23 + 52; Bottom: 30 + 053 + 50 + 23 + 113),
    (Left: 73 + 52; Top: 188 + 113; Right: 45 + 038 + 23 + 52; Bottom: 30 + 075 + 50 + 23 + 113),
    (Left: 70 + 52; Top: 224 + 113; Right: 50 + 038 + 23 + 52; Bottom: 30 + 057 + 50 + 23 + 113));
    pt2: (
    (Left: 42 + 52; Top: 91 + 113; Right: 35 + 047 + 28 + 038 + 52; Bottom: 30 + 005 + 12 + 50 + 113),
    (Left: 124 + 52; Top: 123 + 113; Right: 35 + 044 + 28 + 038 + 52; Bottom: 30 + 035 + 12 + 50 + 113),
    (Left: 41 + 52; Top: 160 + 113; Right: 37 - 034 + 28 + 038 + 52; Bottom: 30 + 058 + 12 + 50 + 113),
    (Left: 112 + 52; Top: 194 + 113; Right: 45 - 037 + 28 + 038 + 52; Bottom: 30 + 082 + 12 + 50 + 113),
    (Left: 107 + 52; Top: 229 + 113; Right: 50 + 038 + 28 + 038 + 52; Bottom: 30 + 063 + 12 + 50 + 113));
    str1: ('???', '????', '????', '????', '????');
    Hint: ('??????????+1', '??????????+1', '', '', '');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    ),
    (
    pt1: (
    (Left: 86 + 52; Top: 70 + 113; Right: 35 + 038 + 23 + 52; Bottom: 30 + 000 + 50 + 23 + 113),
    (Left: 86 + 52; Top: 89 + 113; Right: 35 + 038 + 23 + 52; Bottom: 30 + 028 + 50 + 23 + 113),
    (Left: 86 + 52; Top: 110 + 113; Right: 37 + 038 + 23 + 52; Bottom: 30 + 053 + 50 + 23 + 113),
    (Left: 86 + 52; Top: 134 + 113; Right: 45 + 038 + 23 + 52; Bottom: 30 + 075 + 50 + 23 + 113),
    (Left: 86 + 52; Top: 158 + 113; Right: 50 + 038 + 23 + 52; Bottom: 30 + 057 + 50 + 23 + 113));
    pt2: (
    (Left: 42 + 52; Top: 74 + 113; Right: 35 + 047 + 28 + 038 + 52; Bottom: 30 + 005 + 12 + 50 + 113),
    (Left: 132 + 52; Top: 95 + 113; Right: 35 + 044 + 28 + 038 + 52; Bottom: 30 + 035 + 12 + 50 + 113),
    (Left: 42 + 52; Top: 116 + 113; Right: 37 - 034 + 28 + 038 + 52; Bottom: 30 + 058 + 12 + 50 + 113),
    (Left: 132 + 52; Top: 141 + 113; Right: 45 - 037 + 28 + 038 + 52; Bottom: 30 + 082 + 12 + 50 + 113),
    (Left: 42 + 52; Top: 164 + 113; Right: 50 + 038 + 28 + 038 + 52; Bottom: 30 + 063 + 12 + 50 + 113));
    str1: ('???', '???', '??', '????', '????');
    Hint: ('', '', '', '', '');
    //moved: (False, False, False, False, False);
    //downed: (False, False, False, False, False);
    )
    );
{$ENDIF}

{$IFEND SERIESSKILL}

  g_boFlashMission: Boolean = False;
  g_boNewMission: Boolean = False;
  g_dwNewMission: Longword = 0;

  g_asSkillDesc: array[1..255] of string = (
    '?????????????????????\???????',
    '???????????????????\?????????',
    '??????????????????',
    '???????????????????\???????????????',
    '????????????????????\???????',
    '???????????????????\??????',
    '??????????????????',
    '??????????????????',
    '??????????????????\??????????????????',
    '??????????????????\???????????',
    '????????????????????', {11}
    '???????????????????\??????',
    '???????????????????????\?????????',
    '??????????????????\???????????',
    '??????????????????\???????',
    '?????????????????????\?????????????',
    '??????????????????\?????????????????????',
    '???????????????????\????????????????',
    '???????????????????\?????????????',
    '???????????????????\????????????????????????',
    '??????????????????\??????????????????', {21}
    '??????????????????\???????????',
    '???????????????????\??????????????',
    '????????????????????\??????????????????????',
    '????????????????????\??????????',
    '??????????????????\????????????????',
    '?????????????????\???????????????????',
    '?????????????????',
    '???????????????????\????',
    '??????????????????\?????????????',
    '????????????????????\??????????????????', {31}
    '???????????????????',
    '???????????????????\????????????????',
    '?????????????????',
    '', {??????}
    '???????????????????\???????????????????', {?????}
    '????????????????\????????????????',
    '???????????????????\?????????????',
    '???????????????????',
    '??????????????????\????????????',
    '?????????????????\?????????????', {41}
    '?????????????????\????????????????',
    '?????????????????\???????????????????\?????????????????',
    '?????????????????\?????????????????',
    '???????????MP????\??????????????',
    '',
    '????????????????????????',
    '????????????????????\????????????????????????',
    '?????????????????????',
    '?????????????????',
    '????', {51}
    '??????',
    '???',
    '??????',
    '',
    '?????????????????\??????????????????',
    '???????????????????\????????',
    '???????????????\????????????????????',
    '', {59}
    '????????????????????\?????????????????',
    '??????????????????\????????????????????',
    '????????????????????\???????????????',
    '??????????????????\???????',
    '???????????????????\??????????????????????',
    '????????????????????????',
    '?????????????????????\???????????????????????',
    '', //67
    '', //68
    '', //69
    '????????????????????', //70
    '??????????????????', //71
    '?????????????????????', //72
    '', //73
    '', //74
    '??????????????????\??????', //75
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '????????????????????\?????????????????????', //100
    '?????????????????\????????????????????????', //101
    '?????????????????????\?????????????????\?????????????', //102
    '???????????????????\?????????????????????\??5*5???????????', //103

    '?????????????????????\??????????????????????', //104
    '???????????????????\??????????????????????', //105
    '???????????????????????\????????????????????\??5*5??????????????', //106
    '????????????????????\??????????????????????', //107

    '????????????????\??????????????????????', //108
    '??????????????????????????\??????????????????????', //109
    '????????????????????\??????????????????????', //110
    '????????????\????????????????????\??5*5???????????', //111
    '????????????????????\??3*3??????????????',
    '???????????????????\????????????????\?????????????',
    '???????????????????\????????????????\???????????',
    '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''
    );

const
//  g_sGameConfigBackGround = '.\Data\ui\gcbkd.uib';
//  g_sGameConfigOk1 = '.\Data\ui\gcok1.uib';
//  g_sGameConfigOk2 = '.\Data\ui\gcok2.uib';
//  g_sGameConfigPage1 = '.\Data\ui\gcpage1.uib';
//  g_sGameConfigPage2 = '.\Data\ui\gcpage2.uib';
//  g_sGameConfigClose1 = '.\Data\ui\gcclose1.uib';
//  g_sGameConfigClose2 = '.\Data\ui\gcclose2.uib';
//  g_sGameConfigCheckBox1 = '.\Data\ui\gccheckbox1.uib';
//  g_sGameConfigCheckBox2 = '.\Data\ui\gccheckbox2.uib';

  g_sGameConfigBackGround = 40; //???
//  g_sGameConfigOk1 = 2;
//  g_sGameConfigOk2 = 3;
  g_sGameConfigPage1 = 44; //???
  g_sGameConfigPage2 = 45;
  g_sGameConfigClose1 = 46;//???
  g_sGameConfigClose2 = 47;
  g_sGameConfigCheckBox1 = 48; //???
  g_sGameConfigCheckBox2 = 49; //???

  g_sBookPage = '.\Data\books\%d\%d.uib';
  g_sBookBtn = '.\Data\books\%d\%s.uib';
//  g_sMiniMap = '.\Data\minimap\%d.mmap';


  g_sStateWindowHero = 50; //???
  g_sHeroStatusWindow = 51; //???

  g_sDscStart0 = 60; //???
  g_sDscStart1 = 61; //???
  g_sGloryButton = 62;//???

  g_sVigourbar1 = 64;//???
  g_sVigourbar2 = 65;

  g_sBookBkgnd = 70;  //???
  g_sBookCloseNormal = 71;//???
  g_sBookCloseDown = 72; //???
  g_sBookNextPageNormal = 73;
  g_sBookNextPageDown = 74;
  g_sBookPrevPageNormal = 75;
  g_sBookPrevPageDown = 76;

  g_WStall = 80; //???
  g_WStallPrice = 81; //???
  g_PStallPrice0 = 82; //???
  g_PStallPrice1 = 83; //???
  g_StallBot0 = 84;  //???
  g_StallBot1 = 85;//???
  g_StallLooks = 80; //???

  g_affiche0 = '???????????';
  g_affiche1 = '???????????';
  g_affiche2 = '?????????? ?????????? ?????????? ?????????? ??????????';
  g_affiche3 = '??????????? ??????????? ??????????? ?????????? ?????????';

const
  WH_KEYBOARD_LL = 13;
  LLKHF_ALTDOWN = $20;

type
  TMirStartupInfo = record
    sServerName: String[30];
    sServeraddr: String[30];
    sServerKey: String[100];
    sUIPakKey: String[32];
    sResourceDir: String[50];
    nServerPort: Integer;
    boFullScreen: Boolean;
    boWaitVBlank: Boolean;
    bo3D: Boolean;
    boMini: Boolean;
    nScreenWidth: Integer;
    nScreenHegiht: Integer;
    nLocalMiniPort: Integer;
    btClientVer: Byte;
    sLogo: String[255];
    PassWordFileName:string[127];
  end;

  TMiniResRequest = packed record
    _Type: Byte; //0:?? 1:?????
    Important: Boolean; //???????????????????????????
    FileName: String[200];
    Index: Integer;
    FailCount:Word; //??????
    Data:Pointer; //??????????????
  end;
  PTMiniResRequest = ^TMiniResRequest;

  TMiniResResponse = packed record
    Ident: Integer;
    FileName: String[200];
    Index: Integer;
    Position: Integer;
  end;
  PTMiniResResponse = ^TMiniResResponse;

  PFindNOde = ^TFindNode;
  TFindNode = record
    X, Y: Integer; //????
  end;

  PTree = ^Tree;
  Tree = record
    H: Integer;
    X, Y: Integer;
    Dir: Byte;
    Father: PTree;
  end;

  PLink = ^Link;
  Link = record
    Node: PTree;
    F: Integer;
    Next: PLink;
  end;

  TVirusSign = packed record
    Offset: Integer;
    //Length: LongWord;
    CodeSign: string;
  end;
  pTVirusSign = ^TVirusSign;

  tagKBDLLHOOKSTRUCT = packed record
    vkCode: DWORD;
    scanCode: DWORD;
    flags: DWORD;
    Time: DWORD;
    dwExtraInfo: DWORD;
  end;
  KBDLLHOOKSTRUCT = tagKBDLLHOOKSTRUCT;
  PKBDLLHOOKSTRUCT = ^KBDLLHOOKSTRUCT;

  TTimerCommand = (tcSoftClose, tcReSelConnect, tcFastQueryChr, tcQueryItemPrice {, tcGetToSelChar});
  TChrAction = (caWalk, caRun, caHorseRun, caHit, caSpell, caSitdown);
  TConnectionStep = (cnsIntro, cnsLogin, cnsSelChr, cnsReSelChr, cnsPlay);
  TAPPass = array[0..MAXX * 3, 0..MAXY * 3] of DWORD;
  PTAPPass = ^TAPPass;

//  TDXFont = packed record       //ASP???
//    //boBold: Boolean;
//    clFront: Integer;
//    pszText: string;
//    dwLaststTick: Longword;
//    pSurface: TDirectDrawSurface;
//  end;
//  PTDXFont = ^TDXFont;

  TMovingItem = record
    Index: Integer;
    Item: TClientItem;
  end;
  pTMovingItem = ^TMovingItem;

  TCleintBox = packed record
    Index: Integer;
    Item: TClientItem;
  end;

  TItemType = (i_HPDurg, i_MPDurg, i_HPMPDurg, i_OtherDurg, i_Weapon, i_Dress, i_Helmet, i_Necklace, i_Armring, i_Ring, i_Belt, i_Boots, i_Charm, i_Book, i_PosionDurg, i_UseItem, i_Scroll, i_Stone, i_Gold, i_Other);
  TShowItem = record
    sItemName: string;
    ItemType: TItemType;
    boAutoPickup: Boolean;
    boShowName: Boolean;
    nFColor: Integer;
    nBColor: Integer;
  end;
  pTShowItem = ^TShowItem;
  TControlInfo = record
    Image: Integer;
    Left: Integer;
    Top: Integer;
    Width: Integer;
    Height: Integer;
    Obj: TDControl;
  end;
  pTControlInfo = ^TControlInfo;
  TConfig = record
    DMsgDlg: TControlInfo;
    DMsgDlgOk: TControlInfo;
    DMsgDlgYes: TControlInfo;
    DMsgDlgCancel: TControlInfo;
    DMsgDlgNo: TControlInfo;
    DLogin: TControlInfo;
    DLoginNew: TControlInfo;
    DLoginOk: TControlInfo;
    DLoginChgPw: TControlInfo;
    DLoginClose: TControlInfo;
    DSelServerDlg: TControlInfo;
    DSSrvClose: TControlInfo;
    DSServer1: TControlInfo;
    DSServer2: TControlInfo;
    DSServer3: TControlInfo;
    DSServer4: TControlInfo;
    DSServer5: TControlInfo;
    DSServer6: TControlInfo;
    DNewAccount: TControlInfo;
    DNewAccountOk: TControlInfo;
    DNewAccountCancel: TControlInfo;
    DNewAccountClose: TControlInfo;
    DChgPw: TControlInfo;
    DChgpwOk: TControlInfo;
    DChgpwCancel: TControlInfo;
    DSelectChr: TControlInfo;
    DBottom: TControlInfo;
    DMyState: TControlInfo;
    DMyBag: TControlInfo;
    DMyMagic: TControlInfo;
    DOption: TControlInfo;
    DBotMiniMap: TControlInfo;
    DBotTrade: TControlInfo;
    DBotGuild: TControlInfo;
    DBotGroup: TControlInfo;
    DBotFriend: TControlInfo;
    DBotDare: TControlInfo;
    DBotLevelRank: TControlInfo;
    DBotPlusAbil: TControlInfo;
    //DBotMemo: TControlInfo;
    //DBotExit: TControlInfo;
    //DBotLogout: TControlInfo;
    DBelt1: TControlInfo;
    DBelt2: TControlInfo;
    DBelt3: TControlInfo;
    DBelt4: TControlInfo;
    DBelt5: TControlInfo;
    DBelt6: TControlInfo;
    DGold: TControlInfo;
    DRfineItem: TControlInfo;
    DCloseBag: TControlInfo;
    DMerchantDlg: TControlInfo;
    DMerchantDlgClose: TControlInfo;
    DConfigDlg: TControlInfo;
    DConfigDlgOK: TControlInfo;
    DConfigDlgClose: TControlInfo;
    DMenuDlg: TControlInfo;
    DMenuPrev: TControlInfo;
    DMenuNext: TControlInfo;
    DMenuBuy: TControlInfo;
    DMenuClose: TControlInfo;
    DSellDlg: TControlInfo;
    DSellDlgOk: TControlInfo;
    DSellDlgClose: TControlInfo;
    DSellDlgSpot: TControlInfo;
    DKeySelDlg: TControlInfo;
    DKsIcon: TControlInfo;
    DKsF1: TControlInfo;
    DKsF2: TControlInfo;
    DKsF3: TControlInfo;
    DKsF4: TControlInfo;
    DKsF5: TControlInfo;
    DKsF6: TControlInfo;
    DKsF7: TControlInfo;
    DKsF8: TControlInfo;
    DKsConF1: TControlInfo;
    DKsConF2: TControlInfo;
    DKsConF3: TControlInfo;
    DKsConF4: TControlInfo;
    DKsConF5: TControlInfo;
    DKsConF6: TControlInfo;
    DKsConF7: TControlInfo;
    DKsConF8: TControlInfo;
    DKsNone: TControlInfo;
    DKsOk: TControlInfo;
    DChgGamePwd: TControlInfo;
    DChgGamePwdClose: TControlInfo;
    DItemGrid: TControlInfo;
  end;

  TMapDescInfo = record
    szMapTitle: string;
    szPlaceName: string;
    nPointX: Integer;
    nPointY: Integer;
    nColor: TColor;
    nFullMap: Integer;
  end;
  pTMapDescInfo = ^TMapDescInfo;

//  TDXImgInf = record         //ASP???
//    DImageObj: TDXImages;
//    dwImgTick: Longword;
//    PDImgList: TGList;
//  end;
//  PTDXImgInf = ^TDXImgInf;

  TItemShine = record
    idx: Integer;
    tick: Longword;
  end;

  TTempSeriesSkill = array[0..3] of Byte;

  TSeriesSkill = record
    wMagid: Byte;
    nStep: Byte;
    bSpell: Boolean;
  end;

  TTempSeriesSkillA = record
    pm: PTClientMagic;
    bo: Boolean;
  end;
  PTTempSeriesSkillA = ^TTempSeriesSkillA;

var
  ClientParamStr : string;
  g_Application: IApplication = nil;
  g_pWsockAddr: array[0..4] of Byte;
  g_dwImgThreadId: Longword = 0;
  g_hImagesThread: THandle = INVALID_HANDLE_VALUE;

  g_LogoHide: Boolean = False;
  g_nMagicRange: Integer = 8;
  g_TileMapOffSetX: Integer = 9;
  g_TileMapOffSetY: Integer = 9;

  g_btMyEnergy: Byte = 0;
  g_btMyLuck: Byte = 0;

  g_tiOKShow: TItemShine = (idx: 0; tick: 0);
  g_tiFailShow: TItemShine = (idx: 0; tick: 0);
  g_tiOKShow2: TItemShine = (idx: 0; tick: 0);
  g_tiFailShow2: TItemShine = (idx: 0; tick: 0);

  g_spOKShow2: TItemShine = (idx: 0; tick: 0);
  g_spFailShow2: TItemShine = (idx: 0; tick: 0);

  g_tiHintStr1: string = '';
  g_tiHintStr2: string = '';
  g_TIItems: array[0..1] of TMovingItem;

  g_spHintStr1: string = '';
  g_spHintStr2: string = '';
  g_spItems: array[0..1] of TMovingItem;

  g_SkidAD_Count: Integer = 0;
  g_SkidAD_Count2: Integer = 0;
  g_lastHeroSel: string;
  g_heros: THerosInfo;

  g_ReSelChr: Boolean = False;
  g_Logined: Boolean = False;
  g_ItemWear: Byte = 0;
  g_ShowSuite: Byte = 0;
  g_ShowSuite2: Byte = 0;
  g_ShowSuite3: Byte = 0;

  g_SuiteSpSkill: Byte = 0;

  g_SuiteIdx: Integer = -1;
  g_SuiteItemsList: TList;
  g_pshine: TSimpleMsgPack;
  g_TitlesList: TList;
  g_btSellType: Byte = 0;
  g_showgamegoldinfo: Boolean = False;
  SSE_AVAILABLE: Boolean = False;
  g_lWavMaxVol: Integer = 68; //-100;

  g_uDressEffectTick: Longword;
  g_sDressEffectTick: Longword;
  g_hDressEffectTick: Longword;

  g_uDressEffectIdx: Integer;
  g_sDressEffectIdx: Integer;
  g_hDressEffectIdx: Integer;

  g_uWeaponEffectTick: Longword;
  g_sWeaponEffectTick: Longword;
  g_hWeaponEffectTick: Longword;

  g_uWeaponEffectIdx: Integer;
  g_sWeaponEffectIdx: Integer;
  g_hWeaponEffectIdx: Integer;

  g_ChatStatusLarge: BOOL = False;
  g_ChatWindowLines: Integer = 12;

  g_LoadBeltConfig: BOOL = False;
  g_BeltMode: BOOL = True;
  g_BeltPositionX: Integer = 408;
  g_BeltPositionY: Integer = 487;

  g_dwActorLimit: Integer = 5;
  g_nProcActorIDx: Integer = 0;

  g_boPointFlash: Boolean = False;
  g_PointFlashTick: Longword;
  g_boHPointFlash: Boolean = False;
  g_HPointFlashTick: Longword;

  g_VenationInfos: TVenationInfos = (
    (Level: 0; Point: 0),
    (Level: 0; Point: 0),
    (Level: 0; Point: 0),
    (Level: 0; Point: 0)
    ); //???????
  g_hVenationInfos: TVenationInfos = (
    (Level: 0; Point: 0),
    (Level: 0; Point: 0),
    (Level: 0; Point: 0),
    (Level: 0; Point: 0)
    ); //???????

  g_NextSeriesSkill: Boolean = False;
  g_dwSeriesSkillReadyTick: Longword;
  g_nCurrentMagic: Integer = 888;
  g_nCurrentMagic2: Integer = 888;
  g_SendFireSerieSkillTick: Longword;
  g_IPointLessHintTick: Longword;
  g_MPLessHintTick: Longword;
  g_SeriesSkillStep: Integer = 0;
  g_SeriesSkillFire_100: Boolean = False;
  g_SeriesSkillFire: Boolean = False;
  g_SeriesSkillReady: Boolean = False;
  g_SeriesSkillReadyFlash: Boolean = False;
  //g_TempMagicArr            : array[0..3] of TTempSeriesSkillA;
  g_TempSeriesSkillArr: TTempSeriesSkill;
  g_HTempSeriesSkillArr: TTempSeriesSkill;
  g_SeriesSkillArr: array[0..3] of Byte; //TSeriesSkill;
  g_SeriesSkillSelList: TStringList;
  g_hSeriesSkillSelList: TStringList;

  g_dwAutoTecTick: Longword;
  g_dwAutoTecHeroTick: Longword;
  g_ProcOnIdleTick: Longword;

  g_dwProcMessagePacket: Longword;
  g_ProcNowTick: Longword;
  g_ProcCanFill: Boolean = True;

  g_ProcOnDrawTick: Longword;
  g_ProcOnDrawTick_Effect: Longword;
  g_ProcOnDrawTick_Effect2: Longword;

//  g_ProcCanDraw: Boolean;
//  FFrameRate: Integer;
//  FInterval: Cardinal;
//  FInterval2: Cardinal;
//  FNowFrameRate: Integer;
//  FOldTime: DWORD;
//  FOldTime2: DWORD;
  //g_ProcCanDraw_Effect      : Boolean;
  //g_ProcCanDraw_Effect2     : Boolean;

  g_dwImgMgrTick: Longword;
  g_nImgMgrIdx: Integer = 0;
  //ProcImagesCS              : TRTLCriticalSection;
  ProcMsgCS: TRTLCriticalSection;
  ThreadCS: TRTLCriticalSection;
  g_bIMGBusy: Boolean = False;
  g_DsMiniMapPixel: TCustomLockableTexture = nil;
  g_MainFrom: TForm;
  //g_dwCurrentTick           : PLongWord;
  g_dwThreadTick: PLongWord;
  g_rtime: Longword = 0;
  g_dwLastThreadTick: Longword = 0;

  g_boExchgPoison: Boolean = False;
  g_boCheckTakeOffPoison: Boolean;
  g_Angle: Integer = 0;
  g_ShowMiniMapXY: Boolean = False;
  g_DrawingMiniMap: Boolean = False;
  g_DrawMiniBlend: Boolean = False;
  g_MiniMapRC: TIntRect;
  g_boTakeOnPos: Boolean = True;
  g_boHeroTakeOnPos: Boolean = True;

  g_pRcHeader: pTRcHeader;
//  g_bLoginKey: PBoolean;
//  g_pbInitSock: PBoolean;
  g_pbRecallHero: PBoolean;
  g_RareBoxWindow: TRareBoxWindow;
  //g_dwCheckTick             : LongWord = 0;
  g_dwCheckCount: Integer = 0;
  g_nBookPath: Integer = 0;
  g_nBookPage: Integer = 0;
  g_HillMerchant: Integer = 0;
  g_sBookLabel: string = '';

  g_MaxExpFilter: Integer = 2000;
  g_boDrawLevelRank: Boolean = False;
  g_HeroLevelRanks: THeroLevelRanks;
  g_HumanLevelRanks: THumanLevelRanks;

  g_UnBindItems: array[0..12] of string = (
    '??????',
    '?????',
    '??????',
    '?????',
    '??????',
    '???(???)',
    '????(???)',
    '???(????)',
    '????(????)',
    '?????????',
    '????????',
    '????',
    '??????');
  g_sLogoText: string = 'LegendSoft';
  g_sGoldName: string = '???';
  g_sGameGoldName: string = '???';
  g_sGamePointName: string = '???';
  g_sWarriorName: string = '???'; //??????
  g_sWizardName: string = '????'; //??????
  g_sTaoistName: string = '???'; //??????
  g_sUnKnowName: string = '??';

  g_sMainParam1: string; //??????????
  g_sMainParam2: string; //??????????
  g_sMainParam3: string; //??????????
  g_sMainParam4: string; //??????????
  g_sMainParam5: string; //??????????
  g_sMainParam6: string; //??????????
  g_LoginKey: string = 'password';
  g_PakPassword: string;
  g_boQueryDynCode :Boolean = False;

//  g_DXDraw: TDXDraw;
//  g_DWinMan: TDWinManager;
  //g_DXSound                 : TDXSound;
  //g_Sound                   : TSoundEngine;

  g_wui: TWMImages;
  g_opui: TWMImages;
  g_WMainImages: TWMImages;
  g_WMain2Images: TWMImages;
  g_WMain3Images: TWMImages;
  g_WChrSelImages: TWMImages;

  g_WMonImg: TWMImages;
  g_WMon2Img: TWMImages;
  g_WMon3Img: TWMImages;
  g_WMon4Img: TWMImages;
  g_WMon5Img: TWMImages;
  g_WMon6Img: TWMImages;
  g_WMon7Img: TWMImages;
  g_WMon8Img: TWMImages;
  g_WMon9Img: TWMImages;
  g_WMon10Img: TWMImages;
  g_WMon11Img: TWMImages;
  g_WMon12Img: TWMImages;
  g_WMon13Img: TWMImages;
  g_WMon14Img: TWMImages;
  g_WMon15Img: TWMImages;
  g_WMon16Img: TWMImages;
  g_WMon17Img: TWMImages;
  g_WMon18Img: TWMImages;
  g_WMon19Img: TWMImages;
  g_WMon20Img: TWMImages;
  g_WMon21Img: TWMImages;
  g_WMon22Img: TWMImages;
  g_WMon23Img: TWMImages;
  g_WMon24Img: TWMImages;
  g_WMon25Img: TWMImages;
  g_WMon26Img: TWMImages;
  g_WMon27Img: TWMImages;
  g_WMon28Img: TWMImages;
  g_WMon29Img: TWMImages;
  g_WMon30Img: TWMImages;
  g_WMon31Img: TWMImages;
  g_WMon32Img: TWMImages;
  g_WMon33Img: TWMImages;
  g_WMon34Img: TWMImages;
  g_WMon35Img: TWMImages;
  g_WMon36Img: TWMImages;
  g_WMon37Img: TWMImages;
  g_WMon38Img: TWMImages;
  g_WMon39Img: TWMImages;
  g_WMon40Img: TWMImages;
  // ???? Mon41-Mon46
  g_WMon41Img: TWMImages;  // ????
  g_WMon42Img: TWMImages;  // ????
  g_WMon43Img: TWMImages;  // ?????
  g_WMon44Img: TWMImages;  // ?????
  g_WMon45Img: TWMImages;  // ????
  g_WMon46Img: TWMImages;  // ????

  g_WEffectImg: TWMImages;
  g_WDragonImg: TWMImages;
  g_WSkeletonImg: TWMImages;

 // g_WBooksImages: TWMImages;
 // g_WMainUibImages: TWMImages;
 // g_WMiniMapImages: TWMImages;

  g_WMMapImages: TWMImages;
  //g_WTilesImages            : TWMImages;
  //g_WSmTilesImages          : TWMImages;
  //g_WTiles2Images           : TWMImages;
  //g_WSmTiles2Images         : TWMImages;
  g_WHumWingImages: TWMImages;
  g_WHumEffect2: TWMImages;
  g_WHumEffect3: TWMImages;
  g_WBagItemImages: TWMImages;
  g_WBagItemImages2: TWMImages;
  g_WStateItemImages: TWMImages;
  g_WStateItemImages2: TWMImages;
  g_WDnItemImages: TWMImages;
  g_WDnItemImages2: TWMImages;
  g_WAccessoryImages: TWMImages;  // 配饰装备图标 (戒指、靴子、腰带)
  g_WHumImgImages: TWMImages;
  g_WHum2ImgImages: TWMImages;
  g_WHum3ImgImages: TWMImages;
  g_WHum4ImgImages: TWMImages;  // 新增衣服资源 (Shape 50-63)
  g_ArmorEffectImages: TWMImages;  // 衣服装备特效
  g_WHairImgImages: TWMImages;
  g_WHair2ImgImages: TWMImages;
  g_WWeaponImages: TWMImages;
  g_WWeapon2Images: TWMImages;
  g_WWeapon3Images: TWMImages;
  g_WMagIconImages: TWMImages;
  g_WMagIcon2Images: TWMImages;
  g_WNpcImgImages: TWMImages;
  g_WNpc2ImgImages: TWMImages;
  g_WMagicImages: TWMImages;
  g_WMagic2Images: TWMImages;
  g_WMagic3Images: TWMImages;
  g_WMagic4Images: TWMImages;
  g_WMagic5Images: TWMImages;
  g_WMagic6Images: TWMImages;
  g_WMagic7Images: TWMImages;
  g_WMagic7Images2: TWMImages;
  g_WMagic8Images: TWMImages;
  g_WMagic8Images2: TWMImages;
  g_WMagic9Images: TWMImages = nil;
  g_WMagic10Images: TWMImages = nil;
  g_StateEffect: TWMImages;
  g_ShineEffect: TWMImages;
  //g_Shineitem: TWMImages;
  g_ShineItems: TWMImages;
  g_ShineDnItems: TWMImages;
  //??????
  g_WAniTilesArr: TWMImages;
{$IF CUSTOMLIBFILE = 1}
  g_WEventEffectImages: TWMImages;
{$IFEND}
  g_WObjectArr: array[0..255] of TWMImages;
  g_WTilesArr: array[0..255] of TWMImages;
  g_WSmTilesArr: array[0..255] of TWMImages;

  g_cboEffect: TWMImages;
  g_cbohair: TWMImages;
  g_cbohum: TWMImages;
  g_cbohum3: TWMImages;
  g_cboHumEffect: TWMImages;
  g_cboHumEffect2: TWMImages;
  g_cboHumEffect3: TWMImages;
  g_cboweapon: TWMImages;
  g_cboweapon3: TWMImages;

  g_PowerBlock: TPowerBlock = (
    $55, $8B, $EC, $83, $C4, $E8, $89, $55, $F8, $89, $45, $FC, $C7, $45, $EC, $E8,
    $03, $00, $00, $C7, $45, $E8, $64, $00, $00, $00, $DB, $45, $EC, $DB, $45, $E8,
    $DE, $F9, $DB, $45, $FC, $DE, $C9, $DD, $5D, $F0, $9B, $8B, $45, $F8, $8B, $00,
    $8B, $55, $F8, $89, $02, $DD, $45, $F0, $8B, $E5, $5D, $C3,
    $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00,
    $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00,
    $00, $00, $00, $00, $00, $00, $00, $00
    );
  g_PowerBlock1: TPowerBlock = (
    $55, $8B, $EC, $83, $C4, $E8, $89, $55, $F8, $89, $45, $FC, $C7, $45, $EC, $64,
    $00, $00, $00, $C7, $45, $E8, $64, $00, $00, $00, $DB, $45, $EC, $DB, $45, $E8,
    $DE, $F9, $DB, $45, $FC, $DE, $C9, $DD, $5D, $F0, $9B, $8B, $45, $F8, $8B, $00,
    $8B, $55, $F8, $89, $02, $DD, $45, $F0, $8B, $E5, $5D, $C3,
    $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00,
    $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00, $00,
    $00, $00, $00, $00, $00, $00, $00, $00
    );
  //g_RegInfo                 : TRegInfo;
  g_sServerName: string; //?????????????
  g_sServerMiniName: string; //??????????
  g_psServerAddr: PString;
  g_sSelChrAddr: string;
  g_nSelChrPort: Integer;
  g_sRunServerAddr: string;
  g_nRunServerPort: Integer;
  g_nTopDrawPos: Integer = 0;
  g_nLeftDrawPos: Integer = 0;

  g_boForceMapDraw: Boolean = False;
  g_boForceMapFileLoad: Boolean = False;

  g_boSendLogin: Boolean; //??????????
  g_boServerConnected: Boolean;
  g_SoftClosed: Boolean;
  g_ChrAction: TChrAction;
  g_ConnectionStep: TConnectionStep;
  //g_boSound                 : Boolean;
  //g_boBGSound               : Boolean;

  g_MainFont: string = '????';
  g_FontArr: array[0..MAXFONT - 1] of string = (
    '????',
    '??????',
    '????',
    '????',
    'Courier New',
    'Arial',
    'MS Sans Serif',
    'Microsoft Sans Serif'
    );

  g_OldTime: Longword;
  g_nCurFont: Integer = 0;
  g_sCurFontName: string = '????';

  g_boFullScreen: Boolean = False;
  //g_BitSperpelChanged       : Boolean = False;
  //g_ColorBits               : Integer;
  //g_BitSperpelCount         : Integer = 32;
  //g_nDrawCount              : Integer = 18;

  g_boDXDrawize: Boolean = False;

  g_DlgInitialize: Boolean;
//  g_ImgMixSurface: TCustomLockableTexture;
//  g_ImgLargeMixSurface: TCustomLockableTexture;
//  g_MiniMapSurface: TCustomLockableTexture;

  g_HeathBar_Red: TCustomLockableTexture;
  g_HeathBar_Green: TCustomLockableTexture;
  g_HeathBar_Blue: TCustomLockableTexture;
  g_HeathBar_Yellow: TCustomLockableTexture;
  g_HintSurface_Y: TCustomLockableTexture;
  g_HintSurface_B: TCustomLockableTexture;
  g_HintSurface_C: TCustomLockableTexture;
  g_HintSurface_Olive: TCustomLockableTexture;
  g_HintSurface_Red: TCustomLockableTexture;

  g_HintSurface_W: TCustomLockableTexture;
  g_BotSurface: TCustomLockableTexture;

  g_boFirstActive: Boolean = True;
  g_boFirstTime: Boolean = False;
  g_sMapTitle: string;
  g_nLastMapMusic: Integer = -1;

  g_SendSayList: TStringList;
  g_SendSayListIdx: Integer = 0;

  g_ServerList: TStringList;
  g_GroupMembers: THStringList;
  g_SoundList: TStringList;
  BGMusicList: TStringList;
  g_DxFontsMgrTick: Longword;
  g_MagicArr: array[0..2, 0..255] of PTClientMagic;

  g_MagicList: TList;
{$IF SERIESSKILL}
  g_MagicList2, g_hMagicList2: TList;
{$IFEND SERIESSKILL}
  g_IPMagicList: TList;

  g_HeroMagicList: TList;
  g_HeroIPMagicList: TList;
  g_ShopListArr: array[0..5] of TList;

  g_SaveItemList: TList;
  g_MenuItemList: TList;
  g_DropedItemList: TList;
  g_ChangeFaceReadyList: TList;
  g_FreeActorList: TList;

  g_PoisonIndex: Integer;
  g_nBonusPoint: Integer;
  g_nSaveBonusPoint: Integer;
  g_BonusTick: TNakedAbility;
  g_BonusAbil: TNakedAbility;
  g_NakedAbil: TNakedAbility;
  g_BonusAbilChg: TNakedAbility;

  g_sGuildName: string;
  g_sGuildRankName: string;

  g_dwLatestJoinAttackTick: Longword; //?????????????

  g_dwLastAttackTick: Longword; //????????(?????????????????????)
  g_dwLastMoveTick: Longword; //?????????
  g_dwLatestSpellTick: Longword; //?????????????
  g_dwLatestFireHitTick: Longword; //???????????
  g_dwLatestSLonHitTick: Longword; //???????????
  g_dwLatestTwinHitTick: Longword;
  g_dwLatestPursueHitTick: Longword;
  g_dwLatestRushHitTick: Longword;
  g_dwLatestSmiteHitTick: Longword;
  g_dwLatestSmiteLongHitTick: Longword;
  g_dwLatestSmiteLongHitTick2: Longword;
  g_dwLatestSmiteLongHitTick3: Longword;
  g_dwLatestSmiteWideHitTick: Longword;
  g_dwLatestSmiteWideHitTick2: Longword;
  g_dwLatestRushRushTick: Longword; //?????????
  //g_dwLatestStruckTick      : LongWord; //??????????
  //g_dwLatestHitTick         : LongWord; //??????????????(???????????????????????)
  //g_dwLatestMagicTick       : LongWord; //??????????(???????????????????????)

  g_dwMagicDelayTime: Longword;
  g_dwMagicPKDelayTime: Longword;

  g_nMouseCurrX: Integer; //????????????????X
  g_nMouseCurrY: Integer; //????????????????Y
  g_nMouseX: Integer; //?????????????????X
  g_nMouseY: Integer; //?????????????????Y

  g_nTargetX: Integer; //???????
  g_nTargetY: Integer; //???????
  g_TargetCret: TActor;
  g_FocusCret: TActor;
  g_MagicTarget: TActor;

  g_APQueue: PLink;
  g_APPathList: TList;
  g_APPass: PTAPPass; //array[0..MAXX * 3, 0..MAXY * 3] of DWORD;
  g_APTagget: TActor;

  ///////////////////////////////
  g_APRunTick: Longword;
  g_APRunTick2: Longword;
  g_AutoPicupItem: pTDropItem = nil;
  g_nAPStatus: Integer;
  g_boAPAutoMove: Boolean;
  g_boLongHit: Boolean = False;
  g_sAPStr: string;
  g_boAPXPAttack: Integer = 0;

  m_dwSpellTick: Longword;
  m_dwRecallTick: Longword;
  m_dwDoubluSCTick: Longword;
  m_dwPoisonTick: Longword;
  m_btMagPassTh: Integer = 0;
  g_nTagCount: Integer = 0;
  m_dwTargetFocusTick: Longword;
  g_APPickUpList: THStringList;
  g_APMobList: THStringList;
  g_ItemDesc: THStringList;

  g_AttackInvTime: Integer = 900;
  g_AttackTarget: TActor;
  g_dwSearchEnemyTick: Longword;
  g_boAllowJointAttack: Boolean = False;

  //////////////////////////////////////////
  g_nAPReLogon: Byte = 0;
  g_nAPrlRecallHero: Boolean = False;
  g_nAPSendSelChr: Boolean = False;
  g_nAPSendNotice: Boolean = False;

  g_nAPReLogonWaitTick: Longword;
  g_nAPReLogonWaitTime: Integer = 10 * 1000;

  g_ApLastSelect: Integer = 0;
  g_nOverAPZone: Integer = 0;
  g_nOverAPZone2: Integer = 0;

  g_APGoBack: Boolean = False;
  g_APGoBack2: Boolean = False;

  g_APStep: Integer = -1;
  g_APStep2: Integer = -1;

  g_APMapPath: array of TPoint;
  g_APMapPath2: array of TPoint;

  g_APLastPoint: TPoint;
  g_APLastPoint2: TPoint;

  g_nApMiniMap: Boolean = False;
  g_dwBlinkTime: Longword;
  g_boViewBlink: Boolean;

  //g_boAttackSlow            : Boolean;  //???????????????????.
  //g_boAttackFast            : Byte = 0;
  //g_boMoveSlow              : Boolean;  //????????????????
  //g_nMoveSlowLevel          : Integer;

  g_boMapMoving: Boolean;
  g_boMapMovingWait: Boolean;
  g_boCheckBadMapMode: Boolean;
  g_boCheckSpeedHackDisplay: Boolean;
  g_boViewMiniMap: Boolean;
  g_nViewMinMapLv: Integer;
  g_nMiniMapIndex: Integer;

  //NPC ???
  g_nCurMerchant: Integer;
  g_nMDlgX: Integer;
  g_nMDlgY: Integer;
  g_nStallX: Integer;
  g_nStallY: Integer;

  g_dwChangeGroupModeTick: Longword;
  g_dwDealActionTick: Longword;
  g_dwQueryMsgTick: Longword;
  g_nDupSelection: Integer;

  g_boAllowGroup: Boolean;

  //??????????
  g_nMySpeedPoint: Integer; //????
  g_nMyHitPoint: Integer; //??
  g_nMyAntiPoison: Integer; //??????
  g_nMyPoisonRecover: Integer; //??????
  g_nMyHealthRecover: Integer; //???????
  g_nMySpellRecover: Integer; //??????
  g_nMyAntiMagic: Integer; //??????
  g_nMyHungryState: Integer; //??????
  g_nMyIPowerRecover: Integer; //??????
  g_nMyAddDamage: Integer;
  g_nMyDecDamage: Integer;
  //g_nMyGameDiamd            : Integer = 0;
  //g_nMyGameGird             : Integer = 0;
  //g_nMyGameGold             : Integer = 0;

  g_nHeroSpeedPoint: Integer; //????
  g_nHeroHitPoint: Integer; //??
  g_nHeroAntiPoison: Integer; //??????
  g_nHeroPoisonRecover: Integer; //??????
  g_nHeroHealthRecover: Integer; //???????
  g_nHeroSpellRecover: Integer; //??????
  g_nHeroAntiMagic: Integer; //??????
  g_nHeroHungryState: Integer; //??????
  g_nHeroBagSize: Integer = 40;
  g_nHeroIPowerRecover: Integer;
  g_nHeroAddDamage: Integer;
  g_nHeroDecDamage: Integer;

  g_wAvailIDDay: Word;
  g_wAvailIDHour: Word;
  g_wAvailIPDay: Word;
  g_wAvailIPHour: Word;

  g_MySelf: THumActor;
  g_MyDrawActor: THumActor;

  g_sAttackMode: string = '';

  sAttackModeOfAll: string = '[???????]';
  sAttackModeOfPeaceful: string = '[?????????]';
  sAttackModeOfDear: string = '[?????????]';
  sAttackModeOfMaster: string = '[????????]';
  sAttackModeOfGroup: string = '[????????]';
  sAttackModeOfGuild: string = '[???????]';
  sAttackModeOfRedWhite: string = '[???????]';

  g_RIWhere: Integer = 0;
  g_RefineItems: array[0..2] of TMovingItem;

  g_BuildAcusesStep: Integer = 0;
  g_BuildAcusesProc: Integer = 0;
  g_BuildAcusesProcTick: Longword;

  g_BuildAcusesSuc: Integer = -1;
  g_BuildAcusesSucFrame: Integer = 0;
  g_BuildAcusesSucFrameTick: Longword;

  g_BuildAcusesFrameTick: Longword;
  g_BuildAcusesFrame: Integer = 0;
  g_BuildAcusesRate: Integer = 0;
  g_BuildAcuses: array[0..7] of TMovingItem;
  g_BAFirstShape: Integer = -1;
  //g_BAFirstShape2           : Integer = -1;
  g_tui: array[0..13] of TClientItem;
  g_UseItems: array[0..U_FASHION] of TClientItem;
  g_HeroUseItems: array[0..U_FASHION] of TClientItem;
  UserState1: TUserStateInfo;

  g_detectItemShine: TItemShine;
  UserState1Shine: array[0..U_FASHION] of TItemShine;
  g_UseItemsShine: array[0..U_FASHION] of TItemShine;
  g_HeroUseItemsShine: array[0..U_FASHION] of TItemShine;

  g_ItemArr: array[0..MAXBAGITEMCL - 1] of TClientItem;
  g_HeroItemArr: array[0..MAXBAGITEMCL - 1] of TClientItem;

  g_ItemArrShine: array[0..MAXBAGITEMCL - 1] of TItemShine;
  g_HeroItemArrShine: array[0..MAXBAGITEMCL - 1] of TItemShine;
  g_StallItemArrShine: array[0..10 - 1] of TItemShine;
  g_uStallItemArrShine: array[0..10 - 1] of TItemShine;

  g_DealItemsShine: array[0..10 - 1] of TItemShine;
  g_DealRemoteItemsShine: array[0..20 - 1] of TItemShine;

  g_MovingItemShine: TItemShine;

  g_boBagLoaded: Boolean;
  //g_boHeroBagLoaded         : Boolean;
  g_boServerChanging: Boolean;

  //???????
  g_ToolMenuHook: HHOOK;
  g_ToolMenuHookLL: HHOOK;
  g_nLastHookKey: Integer;
  g_dwLastHookKeyTime: Longword;

  g_nCaptureSerial: Integer; //??????????
  //g_nSendCount              : Integer; //???????????
  g_nReceiveCount: Integer; //????????????
  g_nTestSendCount: Integer;
  g_nTestReceiveCount: Integer;
  g_nSpellCount: Integer; //??????????
  g_nSpellFailCount: Integer; //????????????
  g_nFireCount: Integer; //
  g_nDebugCount: Integer;
  g_nDebugCount1: Integer;
  g_nDebugCount2: Integer;

  //???????
  g_SellDlgItem: TClientItem;
  g_TakeBackItemWait: TMovingItem;
  g_SellDlgItemSellWait: TMovingItem; //TClientItem;

  g_DetectItem: TClientItem;
  g_DetectItemMineID: Integer = 0;
  g_DealDlgItem: TClientItem;
  g_boQueryPrice: Boolean;
  g_dwQueryPriceTime: Longword;
  g_sSellPriceStr: string;

  //???????
  g_DealItems: array[0..9] of TClientItem;
  g_boYbDealing: Boolean = False;
  g_YbDealInfo: TClientPS;
  g_YbDealItems: array[0..9] of TClientItem;
  g_DealRemoteItems: array[0..19] of TClientItem;
  g_nDealGold: Integer;
  g_nDealRemoteGold: Integer;
  g_boDealEnd: Boolean;
  g_sDealWho: string;
  g_MouseItem: TClientItem;
  g_MouseStateItem: TClientItem;
  g_HeroMouseStateItem: TClientItem;
  g_MouseUserStateItem: TClientItem;
  g_HeroMouseItem: TClientItem;
  g_ClickShopItem: TShopItem;

  g_boItemMoving: Boolean;
  g_MovingItem: TMovingItem;
  g_OpenBoxItem: TMovingItem;
  g_WaitingUseItem: TMovingItem;
  g_WaitingStallItem: TMovingItem;
  g_WaitingDetectItem: TMovingItem;

  g_FocusItem, g_FocusItem2: pTDropItem;
  g_boOpenStallSystem: Boolean = True;
  g_boAutoLongAttack: Boolean = True;
  g_boAutoSay: Boolean = True;
  g_boMutiHero: Boolean = True;
  g_boSkill_114_MP: Boolean = False;
  g_boSkill_68_MP: Boolean = False;
  g_nDayBright: Integer;
  g_nAreaStateValue: Integer;
  g_boNoDarkness: Boolean;
  g_nRunReadyCount: Integer;

  g_boLastViewFog: Boolean = False;
//{$IF VIEWFOG}
  g_boViewFog: Boolean = True; //?????????
  g_boForceNotViewFog: Boolean = False; //??????
//{$ELSE}
//  g_boViewFog: Boolean = False; //?????????
//  g_boForceNotViewFog: Boolean = True; //??????
//{$IFEND VIEWFOG}

  g_EatingItem: TClientItem;
  g_dwEatTime: Longword; //timeout...
  g_dwHeroEatTime: Longword;

  g_dwDizzyDelayStart: Longword;
  g_dwDizzyDelayTime: Longword;

  g_boDoFadeOut: Boolean;
  g_boDoFadeIn: Boolean;
  g_nFadeIndex: Integer;
  g_boDoFastFadeOut: Boolean;

  g_boAutoDig, g_boAutoSit: Boolean; //???????
  g_boSelectMyself: Boolean; //????????????

  //??????????????
  g_dwFirstServerTime: Longword;
  g_dwFirstClientTime: Longword;
  //ServerTimeGap: int64;
  g_nTimeFakeDetectCount: Integer;
  //g_dwSHGetCount            : PLongWord;
  //g_dwSHGetTime             : LongWord;
  //g_dwSHTimerTime           : LongWord;
  //g_nSHFakeCount            : Integer;  //????????????????????????4??????????????

  g_dwLatestClientTime2: Longword;
  g_dwFirstClientTimerTime: Longword; //timer ???
  g_dwLatestClientTimerTime: Longword;
  g_dwFirstClientGetTime: Longword; //gettickcount ???
  g_dwLatestClientGetTime: Longword;
  g_nTimeFakeDetectSum: Integer;
  g_nTimeFakeDetectTimer: Integer;

  g_dwLastestClientGetTime: Longword;

  //????????????
  g_dwDropItemFlashTime: Longword = 8 * 1000; //??????????????
  g_nHitTime: Integer = 1400; //????????????  0820
  g_nItemSpeed: Integer = 60;
  g_dwSpellTime: Longword = 500; //???????????

  g_boHero: Boolean = True;
  g_boOpenAutoPlay: Boolean = True;
  g_DeathColorEffect: TColorEffect = ceRed;
  g_boClientCanSet: Boolean = True;

  g_nEatIteminvTime: Integer = 200;
  g_boCanRunSafeZone: Boolean = True;
  g_boCanRunHuman: Boolean = True;
  g_boCanRunMon: Boolean = True;
  g_boCanRunNpc: Boolean = True;
  g_boCanRunAllInWarZone: Boolean = False;
  g_boCanStartRun: Boolean = True; //?????????????
  g_boParalyCanRun: Boolean = False; //???????????
  g_boParalyCanWalk: Boolean = False; //???????????
  g_boParalyCanHit: Boolean = False; //????????????
  g_boParalyCanSpell: Boolean = False; //????????????

  g_boShowRedHPLable: Boolean = True; //??????
  g_boShowHPNumber: Boolean = True; //??????????
  g_boShowJobLevel: Boolean = True; //????????
  g_boDuraAlert: Boolean = True; //?????????
  g_boMagicLock: Boolean = True; //???????
  g_boSpeedRate: Boolean = False;
  g_boSpeedRateShow: Boolean = False;
  //g_boAutoPuckUpItem        : Boolean = False;

  g_boShowHumanInfo: Boolean = True;
  g_boShowMonsterInfo: Boolean = False;
  g_boShowNpcInfo: Boolean = False;
  //?????????????
  g_boQuickPickup: Boolean = False;
  g_dwAutoPickupTick: Longword;
  g_dwAutoPickupTime: Longword = 100; //???????????
  //g_AutoPickupList          : TList;
  g_MagicLockActor: TActor;
  g_boNextTimePowerHit: Boolean;
  g_boCanLongHit: Boolean;
  g_boCanWideHit: Boolean;
  g_boCanCrsHit: Boolean;
  g_boCanStnHit: Boolean;
  g_boNextTimeFireHit: Boolean;
  g_boNextTimeTwinHit: Boolean;
  g_boNextTimePursueHit: Boolean;
  g_boNextTimeRushHit: Boolean;
  g_boNextTimeSmiteHit: Boolean;
  g_boNextTimeSmiteLongHit: Boolean;
  g_boNextTimeSmiteLongHit2: Boolean;
  g_boNextTimeSmiteLongHit3: Boolean;
  g_boNextTimeSmiteWideHit: Boolean;
  g_boNextTimeSmiteWideHit2: Boolean;

  g_boCanSLonHit: Boolean = False;
  g_boCanSquHit: Boolean;
  g_ShowItemList: THStringList;
  g_boDrawTileMap: Boolean = True;
  g_boDrawDropItem: Boolean = True;

  g_nTestX: Integer = 71;
  g_nTestY: Integer = 212;

  g_nSquHitPoint: Integer = 0;
  g_nMaxSquHitPoint: Integer = 0;

  g_boConfigLoaded: Boolean = False;

  g_dwCollectExpLv: Byte = 0;
  g_boCollectStateShine: Boolean = False;
  g_nCollectStateShine: Integer = 0;
  g_dwCollectStateShineTick: Longword;
  g_dwCollectStateShineTick2: Longword;

  g_dwCollectExp: Longword = 0;
  g_dwCollectExpMax: Longword = 1;
  g_boCollectExpShine: Boolean = False;
  g_boCollectExpShineCount: Integer = 0;
  g_dwCollectExpShineTick: Longword;

  g_dwCollectIpExp: Longword = 0;
  g_dwCollectIpExpMax: Longword = 1;

  //20200726???????
  g_Button_DetectBox: Boolean = False;
  g_Button_Mission: Boolean = False;
  g_Button_Shop: Boolean = False;
  g_Button_Friend: Boolean = False;
  g_Button_PlusAbil: Boolean = False;
  g_Button_baoguo: Boolean = True; //????
  g_ShowWinExpMode: Boolean = False;
  g_IgnoreStruck: Boolean = False;


  g_LightImages: array[0..5] of TCustomLockableTexture;

  DlgConf: TConfig = (
    DBottom: (Image: 1; Left: 0; Top: 0; Width: 0; Height: 0);
    DMyState: (Image: 8; Left: 643; Top: 62; Width: 0; Height: 0);
    DMyBag: (Image: 9; Left: 682; Top: 42; Width: 0; Height: 0);
    DMyMagic: (Image: 10; Left: 722; Top: 22; Width: 0; Height: 0);
    DOption: (Image: 11; Left: 764; Top: 12; Width: 0; Height: 0);

    DBotMiniMap: (Image: 130; Left: 209; Top: 104; Width: 0; Height: 0);
    DBotTrade: (Image: 132; Left: 209 + 30; Top: 104; Width: 0; Height: 0);
    DBotGuild: (Image: 134; Left: 209 + 30 * 2; Top: 104; Width: 0; Height: 0);
    DBotGroup: (Image: 128; Left: 209 + 30 * 3; Top: 104; Width: 0; Height: 0);
    DBotFriend: (Image: 34; Left: 209 + 30 * 4; Top: 104; Width: 0; Height: 0);
    DBotDare: (Image: 36; Left: 209 + 30 * 5; Top: 104; Width: 0; Height: 0);
    DBotLevelRank: (Image: 460; Left: 209 + 30 * 6; Top: 104; Width: 0; Height: 0);

    DBotPlusAbil: (Image: 140; Left: 209 + 30 * 8; Top: 104; Width: 0; Height: 0);
    //DBotMemo: (Image: 532; Left: SCREENWIDTH div 2 + (SCREENWIDTH div 2 - (400 - 353)) {753}; Top: 204; Width: 0; Height: 0);
    //DBotExit: (Image: 138; Left: SCREENWIDTH div 2 + (SCREENWIDTH div 2 - (400 - 160)) {560}; Top: 104; Width: 0; Height: 0);
    //DBotLogout: (Image: 136; Left: SCREENWIDTH div 2 + (SCREENWIDTH div 2 - (400 - 160)) - 30 {560}; Top: 104; Width: 0; Height: 0);
    DBelt1: (Image: 0; Left: 285; Top: 59; Width: 32; Height: 29);
    DBelt2: (Image: 0; Left: 328; Top: 59; Width: 32; Height: 29);
    DBelt3: (Image: 0; Left: 371; Top: 59; Width: 32; Height: 29);
    DBelt4: (Image: 0; Left: 415; Top: 59; Width: 32; Height: 29);
    DBelt5: (Image: 0; Left: 459; Top: 59; Width: 32; Height: 29);
    DBelt6: (Image: 0; Left: 503; Top: 59; Width: 32; Height: 29);
    DGold: (Image: 29; Left: 10; Top: 190; Width: 0; Height: 0);
    DRfineItem: (Image: 26; Left: 254; Top: 183; Width: 48; Height: 22);
    DCloseBag: (Image: 371; Left: 309; Top: 203; Width: 14; Height: 20);
    DMerchantDlg: (Image: 384; Left: 0; Top: 0; Width: 0; Height: 0);
    DMerchantDlgClose: (Image: 64; Left: 399; Top: 1; Width: 0; Height: 0);
    DConfigDlg: (Image: 204; Left: 0; Top: 0; Width: 0; Height: 0);
    DConfigDlgOK: (Image: 361; Left: 514; Top: 287; Width: 0; Height: 0);
    DConfigDlgClose: (Image: 64; Left: 584; Top: 6; Width: 0; Height: 0);
    DMenuDlg: (Image: 385; Left: 138; Top: 163; Width: 0; Height: 0);
    DMenuPrev: (Image: 388; Left: 43; Top: 175; Width: 0; Height: 0);
    DMenuNext: (Image: 387; Left: 90; Top: 175; Width: 0; Height: 0);
    DMenuBuy: (Image: 386; Left: 215; Top: 171; Width: 0; Height: 0);
    DMenuClose: (Image: 64; Left: 291; Top: 0; Width: 0; Height: 0);
    DSellDlg: (Image: 392; Left: 328; Top: 163; Width: 0; Height: 0);
    DSellDlgOk: (Image: 393; Left: 85; Top: 150; Width: 0; Height: 0);
    DSellDlgClose: (Image: 64; Left: 115; Top: 0; Width: 0; Height: 0);
    DSellDlgSpot: (Image: 0; Left: 27; Top: 67; Width: 0; Height: 0);
    DKeySelDlg: (Image: 620; Left: 0; Top: 0; Width: 0; Height: 0);
    DKsIcon: (Image: 0; Left: 51; Top: 31; Width: 0; Height: 0);
    DKsF1: (Image: 232; Left: 25; Top: 78; Width: 0; Height: 0);
    DKsF2: (Image: 234; Left: 57; Top: 78; Width: 0; Height: 0);
    DKsF3: (Image: 236; Left: 89; Top: 78; Width: 0; Height: 0);
    DKsF4: (Image: 238; Left: 121; Top: 78; Width: 0; Height: 0);
    DKsF5: (Image: 240; Left: 160; Top: 78; Width: 0; Height: 0);
    DKsF6: (Image: 242; Left: 192; Top: 78; Width: 0; Height: 0);
    DKsF7: (Image: 244; Left: 224; Top: 78; Width: 0; Height: 0);
    DKsF8: (Image: 246; Left: 256; Top: 78; Width: 0; Height: 0);
    DKsConF1: (Image: 626; Left: 25; Top: 120; Width: 0; Height: 0);
    DKsConF2: (Image: 628; Left: 57; Top: 120; Width: 0; Height: 0);
    DKsConF3: (Image: 630; Left: 89; Top: 120; Width: 0; Height: 0);
    DKsConF4: (Image: 632; Left: 121; Top: 120; Width: 0; Height: 0);
    DKsConF5: (Image: 633; Left: 160; Top: 120; Width: 0; Height: 0);
    DKsConF6: (Image: 634; Left: 192; Top: 120; Width: 0; Height: 0);
    DKsConF7: (Image: 638; Left: 224; Top: 120; Width: 0; Height: 0);
    DKsConF8: (Image: 640; Left: 256; Top: 120; Width: 0; Height: 0);
    DKsNone: (Image: 623; Left: 296; Top: 78; Width: 0; Height: 0);
    DKsOk: (Image: 621; Left: 296; Top: 120; Width: 0; Height: 0);
    DChgGamePwd: (Image: 621; Left: 296; Top: 120; Width: 0; Height: 0);
    DChgGamePwdClose: (Image: 64; Left: 312; Top: 1; Width: 0; Height: 0);
    DItemGrid: (Image: 0; Left: 29; Top: 41; Width: 286; Height: 162);
    );

const
  // Windows 2000/XP multimedia keys (adapted from winuser.h and renamed to avoid potential conflicts)
  // See also: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/WindowsUserInterface/UserInput/VirtualKeyCodes.asp
  _VK_BROWSER_BACK = $A6; // Browser Back key
  _VK_BROWSER_FORWARD = $A7; // Browser Forward key
  _VK_BROWSER_REFRESH = $A8; // Browser Refresh key
  _VK_BROWSER_STOP = $A9; // Browser Stop key
  _VK_BROWSER_SEARCH = $AA; // Browser Search key
  _VK_BROWSER_FAVORITES = $AB; // Browser Favorites key
  _VK_BROWSER_HOME = $AC; // Browser Start and Home key
  _VK_VOLUME_MUTE = $AD; // Volume Mute key
  _VK_VOLUME_DOWN = $AE; // Volume Down key
  _VK_VOLUME_UP = $AF; // Volume Up key
  _VK_MEDIA_NEXT_TRACK = $B0; // Next Track key
  _VK_MEDIA_PREV_TRACK = $B1; // Previous Track key
  _VK_MEDIA_STOP = $B2; // Stop Media key
  _VK_MEDIA_PLAY_PAUSE = $B3; // Play/Pause Media key
  _VK_LAUNCH_MAIL = $B4; // Start Mail key
  _VK_LAUNCH_MEDIA_SELECT = $B5; // Select Media key
  _VK_LAUNCH_APP1 = $B6; // Start Application 1 key
  _VK_LAUNCH_APP2 = $B7; // Start Application 2 key
  // Self-invented names for the extended keys
  NAME_VK_BROWSER_BACK = 'Browser Back';
  NAME_VK_BROWSER_FORWARD = 'Browser Forward';
  NAME_VK_BROWSER_REFRESH = 'Browser Refresh';
  NAME_VK_BROWSER_STOP = 'Browser Stop';
  NAME_VK_BROWSER_SEARCH = 'Browser Search';
  NAME_VK_BROWSER_FAVORITES = 'Browser Favorites';
  NAME_VK_BROWSER_HOME = 'Browser Start/Home';
  NAME_VK_VOLUME_MUTE = 'Volume Mute';
  NAME_VK_VOLUME_DOWN = 'Volume Down';
  NAME_VK_VOLUME_UP = 'Volume Up';
  NAME_VK_MEDIA_NEXT_TRACK = 'Next Track';
  NAME_VK_MEDIA_PREV_TRACK = 'Previous Track';
  NAME_VK_MEDIA_STOP = 'Stop Media';
  NAME_VK_MEDIA_PLAY_PAUSE = 'Play/Pause Media';
  NAME_VK_LAUNCH_MAIL = 'Start Mail';
  NAME_VK_LAUNCH_MEDIA_SELECT = 'Select Media';
  NAME_VK_LAUNCH_APP1 = 'Start Application 1';
  NAME_VK_LAUNCH_APP2 = 'Start Application 2';

procedure InitClientItems();
function GetTIHintString1(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
function GetTIHintString2(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
procedure AutoPutOntiBooks();
procedure AutoPutOntiCharms();

function GetSuiteAbil(idx, Shape: Integer; var sa: TtSuiteAbil): Boolean;

procedure AutoPutOntiSecretBooks();

function GetSpHintString1(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
function GetSpHintString2(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;

function GetEvaluationInfo(CurrMouseItem: TClientItem): string;
function GetSecretAbil(CurrMouseItem: TClientItem): Boolean;

procedure InitObj();
procedure LoadItemDesc();
procedure LoadItemFilter();
procedure LoadItemFilter2();
procedure SaveItemFilter();
function getSuiteHint(var idx: Integer; s: string; gender: Byte): pTClientSuiteItems;
function GetItemWhere(Item: TClientItem): Integer;
procedure LoadWMImagesLib();
procedure InitWMImagesLib({DDxDraw: TDXDraw; }bFirst: Boolean);
procedure InitWMImagesLibSecond;
procedure UnLoadWMImagesLib();
procedure LoadLightImages();
procedure UnLoadLightImages();
function GetObjs(nUnit, nIdx: Integer): TCustomLockableTexture;
function GetObjsEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture;

function GetTiles(nUnit, nIdx: Integer): TCustomLockableTexture;
function GetTilesEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture;
function GetSmTiles(nUnit, nIdx: Integer): TCustomLockableTexture;
function GetSmTilesEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture;


function GetMonImg(nAppr: Integer): TWMImages;
//function GetMonAction(nAppr: Integer): pTMonsterAction;
function GetJobName(nJob: Integer): string;
//procedure ClearShowItemList();
function GetItemType(ItemType: TItemType): string;
procedure LoadUserConfig(sUserName: string);
//procedure SaveUserConfig(sUserName: string);
function IsPersentHP(nMin, nMax: Integer): Boolean;
function IsPersentMP(nMin, nMax: Integer): Boolean;
function IsPersentSpc(nMin, nMax: Integer): Boolean;
function IsPersentBook(nMin, nMax: Integer): Boolean;

function IsPersentHPHero(nMin, nMax: Integer): Boolean;
function IsPersentMPHero(nMin, nMax: Integer): Boolean;
function IsPersentSpcHero(nMin, nMax: Integer): Boolean;

function GetHotKey(Modifiers, Key: Word): Cardinal;
procedure SeparateHotKey(HotKey: Cardinal; var Modifiers, Key: Word);
function HotKeyToText(HotKey: Cardinal; Localized: Boolean): string;

procedure CheckSpeedCount(Count: Integer = 2);
function GetSeriesSkillIcon(id: Integer): Integer;
procedure ResetSeriesSkillVar();
//function timeGetTime: DWORD; stdcall;
function GetTickCount: DWORD; //stdcall;

function IsDetectItem(idx: Integer): Boolean;
function IsBagItem(idx: Integer): Boolean;
function IsEquItem(idx: Integer): Boolean;
function IsStorageItem(idx: Integer): Boolean;
function IsStallItem(idx: Integer): Boolean;
function GetLevelColor(iLevel: Byte): Integer;

procedure ScreenChanged();
function ShiftYOffset(): Integer;
procedure InitScreenConfig();

function IsInMyRange(Act: TActor): Boolean;
function IsItemInMyRange(X, Y: Integer): Boolean;

function GetTitle(nItemIdx: Integer): pTClientStdItem;
//function GetGameImages(LoadMode: TLoadMode; const FileName: string): TWMImages;
function FillString(const Str: string; Len: Integer; FillStr: Char): string;
//123456
procedure LoadMapDesc();
procedure SetLoginGateKey(const Value: String);

const
  mmsyst = 'winmm.dll';
  kernel32 = 'kernel32.dll';
  HotKeyAtomPrefix = 'HotKeyManagerHotKey';
  ModName_Shift = 'Shift';
  ModName_Ctrl = 'Ctrl';
  ModName_Alt = 'Alt';
  ModName_Win = 'Win';
  VK2_SHIFT = 32;
  VK2_CONTROL = 64;
  VK2_ALT = 128;
  VK2_WIN = 256;

var
  EnglishKeyboardLayout: HKL;
  ShouldUnloadEnglishKeyboardLayout: Boolean;
  LocalModName_Shift: string = ModName_Shift;
  LocalModName_Ctrl: string = ModName_Ctrl;
  LocalModName_Alt: string = ModName_Alt;
  LocalModName_Win: string = ModName_Win;
  g_MirStartupInfo: TMirStartupInfo;

implementation

uses ClMain, FState, HUtil32,cliUtil, SoundUtil;


procedure SetLoginGateKey(const Value: String);
var
  ADValue: String;
begin
  ADValue := uEDcode.DecodeSource(Value);
  ConsoleDebug(Format('??????:%s',[ADValue]));
  //uLog.TLogger.AddLog('Password:' + ADValue + '$');
  if ADValue <> '' then
    g_LoginKey := ADValue;
end;

function FillString(const Str: string; Len: Integer; FillStr: Char): string;
begin
  Result:= StringOfChar(FillStr, Len - Length(Str)) + Str;
end;

procedure LoadMapDesc();
var
  i: Integer;
  szFileName, szLine: string;
  xsl: TStringList;
  szMapTitle, szPointX, szPointY, szPlaceName, szColor, szFullMap: string;
  nPointX, nPointY, nFullMap: Integer;
  nColor: TColor;
  pMapDescInfo: pTMapDescInfo;
begin
  szFileName := '.\data\MapDesc2.dat';
  if FileExists(szFileName) then begin
    xsl := TStringList.Create;
    xsl.LoadFromFile(szFileName);
    for i := 0 to xsl.Count - 1 do begin
      szLine := xsl[i];
      if (szLine = '') or (szLine[1] = ';') then
        Continue;
      szLine := GetValidStr3(szLine, szMapTitle, [',']);
      szLine := GetValidStr3(szLine, szPointX, [',']);
      szLine := GetValidStr3(szLine, szPointY, [',']);
      szLine := GetValidStr3(szLine, szPlaceName, [',']);
      szLine := GetValidStr3(szLine, szColor, [',']);
      szLine := GetValidStr3(szLine, szFullMap, [',']);
      nPointX := Str_ToInt(szPointX, -1);
      nPointY := Str_ToInt(szPointY, -1);
      nColor := StrToInt(szColor);
      nFullMap := Str_ToInt(szFullMap, -1);
      if (szPlaceName <> '') and (szMapTitle <> '') and (nPointX >= 0) and (nPointY >= 0) and (nFullMap >= 0) then begin
        New(pMapDescInfo);
        pMapDescInfo.szMapTitle := szMapTitle;
        pMapDescInfo.szPlaceName := szPlaceName;
        pMapDescInfo.nPointX := nPointX;
        pMapDescInfo.nPointY := nPointY;
        pMapDescInfo.nColor := nColor;
        pMapDescInfo.nFullMap := nFullMap;
        //DebugOutStr(format('%.8x', [pMapDescInfo.nColor]));
        g_xMapDescList.AddObject(szMapTitle, TObject(pMapDescInfo));
      end;
    end;
    xsl.Free;
  end;
end;

function GetTickCount: DWORD; // external kernel32 name 'GetTickCount';
var
  li: TLargeInteger;
begin
  //Result := g_tTime;
  //if g_tTime = 0 then
  Result := Windows.GetTickCount();
  //Result := timeGetTime;
end;

function IsDetectItem(idx: Integer): Boolean;
begin
  Result := idx = DETECT_MIIDX_OFFSET;
end;

function IsBagItem(idx: Integer): Boolean;
begin
  Result := idx in [6..MAXBAGITEM - 1];
end;

function IsEquItem(idx: Integer): Boolean;
var sel:Integer;
begin
    Result := False;
    if  idx < 0 then
    begin
      sel := -(idx+1);
      Result :=sel in [0..U_FASHION];
    end;

end;

function IsStorageItem(idx: Integer): Boolean;
begin
  Result := (idx >= SAVE_MIIDX_OFFSET) and (idx < SAVE_MIIDX_OFFSET + 46);
end;

function IsStallItem(idx: Integer): Boolean;
begin
  Result := (idx >= STALL_MIIDX_OFFSET) and (idx < STALL_MIIDX_OFFSET + 10);
end;

procedure ResetSeriesSkillVar();
begin
  g_nCurrentMagic := 888;
  g_nCurrentMagic2 := 888;
  g_SeriesSkillStep := 0;
  g_SeriesSkillFire := False;
  g_SeriesSkillFire_100 := False;
  g_SeriesSkillReady := False;
  g_NextSeriesSkill := False;
  FillChar(g_VenationInfos, SizeOf(g_VenationInfos), 0);
  FillChar(g_TempSeriesSkillArr, SizeOf(g_TempSeriesSkillArr), 0);
  FillChar(g_HTempSeriesSkillArr, SizeOf(g_HTempSeriesSkillArr), 0);
  FillChar(g_SeriesSkillArr, SizeOf(g_SeriesSkillArr), 0);
end;

function GetSeriesSkillIcon(id: Integer): Integer;
begin
  Result := -1;
  case id of
    100: Result := 950;
    101: Result := 952;
    102: Result := 956;
    103: Result := 954;

    104: Result := 942;
    105: Result := 946;
    106: Result := 940;
    107: Result := 944;

    108: Result := 934;
    109: Result := 936;
    110: Result := 932;
    111: Result := 930;
    112: Result := 944;
  end;
end;

procedure CheckSpeedCount(Count: Integer);
begin
  Inc(g_dwCheckCount);
  if g_dwCheckCount > Count then begin
    g_dwCheckCount := 0;
    //g_ModuleDetect.FCheckTick := 0;
  end;
end;

function IsExtendedKey(Key: Word): Boolean;
begin
  Result := ((Key >= _VK_BROWSER_BACK) and (Key <= _VK_LAUNCH_APP2));
end;

function GetHotKey(Modifiers, Key: Word): Cardinal;
var
  HK: Cardinal;
begin
  HK := 0;
  if (Modifiers and MOD_ALT) <> 0 then
    Inc(HK, VK2_ALT);
  if (Modifiers and MOD_CONTROL) <> 0 then
    Inc(HK, VK2_CONTROL);
  if (Modifiers and MOD_SHIFT) <> 0 then
    Inc(HK, VK2_SHIFT);
  if (Modifiers and MOD_WIN) <> 0 then
    Inc(HK, VK2_WIN);
  HK := HK shl 8;
  Inc(HK, Key);
  Result := HK;
end;

function HotKeyToText(HotKey: Cardinal; Localized: Boolean): string;

  function GetExtendedVKName(Key: Word): string;
  begin
    case Key of
      _VK_BROWSER_BACK: Result := NAME_VK_BROWSER_BACK;
      _VK_BROWSER_FORWARD: Result := NAME_VK_BROWSER_FORWARD;
      _VK_BROWSER_REFRESH: Result := NAME_VK_BROWSER_REFRESH;
      _VK_BROWSER_STOP: Result := NAME_VK_BROWSER_STOP;
      _VK_BROWSER_SEARCH: Result := NAME_VK_BROWSER_SEARCH;
      _VK_BROWSER_FAVORITES: Result := NAME_VK_BROWSER_FAVORITES;
      _VK_BROWSER_HOME: Result := NAME_VK_BROWSER_HOME;
      _VK_VOLUME_MUTE: Result := NAME_VK_VOLUME_MUTE;
      _VK_VOLUME_DOWN: Result := NAME_VK_VOLUME_DOWN;
      _VK_VOLUME_UP: Result := NAME_VK_VOLUME_UP;
      _VK_MEDIA_NEXT_TRACK: Result := NAME_VK_MEDIA_NEXT_TRACK;
      _VK_MEDIA_PREV_TRACK: Result := NAME_VK_MEDIA_PREV_TRACK;
      _VK_MEDIA_STOP: Result := NAME_VK_MEDIA_STOP;
      _VK_MEDIA_PLAY_PAUSE: Result := NAME_VK_MEDIA_PLAY_PAUSE;
      _VK_LAUNCH_MAIL: Result := NAME_VK_LAUNCH_MAIL;
      _VK_LAUNCH_MEDIA_SELECT: Result := NAME_VK_LAUNCH_MEDIA_SELECT;
      _VK_LAUNCH_APP1: Result := NAME_VK_LAUNCH_APP1;
      _VK_LAUNCH_APP2: Result := NAME_VK_LAUNCH_APP2;
    else
      Result := '';
    end;
  end;

  function GetModifierNames: string;
  var
    s: string;
  begin
    s := '';
    if Localized then begin
      if (HotKey and $4000) <> 0 then // scCtrl
        s := s + LocalModName_Ctrl + '+';
      if (HotKey and $2000) <> 0 then // scShift
        s := s + LocalModName_Shift + '+';
      if (HotKey and $8000) <> 0 then // scAlt
        s := s + LocalModName_Alt + '+';
      if (HotKey and $10000) <> 0 then
        s := s + LocalModName_Win + '+';
    end
    else begin
      if (HotKey and $4000) <> 0 then // scCtrl
        s := s + ModName_Ctrl + '+';
      if (HotKey and $2000) <> 0 then // scShift
        s := s + ModName_Shift + '+';
      if (HotKey and $8000) <> 0 then // scAlt
        s := s + ModName_Alt + '+';
      if (HotKey and $10000) <> 0 then
        s := s + ModName_Win + '+';
    end;
    Result := s;
  end;

  function GetVKName(Special: Boolean): string;
  var
    scanCode: Cardinal;
    KeyName: array[0..255] of Char;
    oldkl: HKL;
    Modifiers, Key: Word;
  begin
    Result := '';
    if Localized then {// Local language key names}  begin
      if Special then
        scanCode := (MapVirtualKey(Byte(HotKey), 0) shl 16) or (1 shl 24)
      else
        scanCode := (MapVirtualKey(Byte(HotKey), 0) shl 16);
      if scanCode <> 0 then begin
        GetKeyNameText(scanCode, KeyName, SizeOf(KeyName));
        Result := KeyName;
      end;
    end
    else {// English key names}  begin
      if Special then
        scanCode := (MapVirtualKeyEx(Byte(HotKey), 0, EnglishKeyboardLayout) shl 16) or (1 shl 24)
      else
        scanCode := (MapVirtualKeyEx(Byte(HotKey), 0, EnglishKeyboardLayout) shl 16);
      if scanCode <> 0 then begin
        oldkl := GetKeyboardLayout(0);
        if oldkl <> EnglishKeyboardLayout then
          ActivateKeyboardLayout(EnglishKeyboardLayout, 0); // Set English kbd. layout
        GetKeyNameText(scanCode, KeyName, SizeOf(KeyName));
        Result := KeyName;
        if oldkl <> EnglishKeyboardLayout then begin
          if ShouldUnloadEnglishKeyboardLayout then
            UnloadKeyboardLayout(EnglishKeyboardLayout); // Restore prev. kbd. layout
          ActivateKeyboardLayout(oldkl, 0);
        end;
      end;
    end;

    if Length(Result) <= 1 then begin
      // Try the internally defined names
      SeparateHotKey(HotKey, Modifiers, Key);
      if IsExtendedKey(Key) then
        Result := GetExtendedVKName(Key);
    end;
  end;

var
  KeyName: string;
begin
  case Byte(HotKey) of
    // PgUp, PgDn, End, Home, Left, Up, Right, Down, Ins, Del
    $21..$28, $2D, $2E: KeyName := GetVKName(True);
  else
    KeyName := GetVKName(False);
  end;
  Result := GetModifierNames + KeyName;
end;

procedure SeparateHotKey(HotKey: Cardinal; var Modifiers, Key: Word);
var
  Virtuals: Integer;
  v: Word;
  X: Word;
begin
  Key := Byte(HotKey);
  X := HotKey shr 8;
  Virtuals := X;
  v := 0;
  if (Virtuals and VK2_WIN) <> 0 then
    Inc(v, MOD_WIN);
  if (Virtuals and VK2_ALT) <> 0 then
    Inc(v, MOD_ALT);
  if (Virtuals and VK2_CONTROL) <> 0 then
    Inc(v, MOD_CONTROL);
  if (Virtuals and VK2_SHIFT) <> 0 then
    Inc(v, MOD_SHIFT);
  Modifiers := v;
end;

function IsPersentHP(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[0]) {or (nMax - nMin > 1500)};
end;

function IsPersentMP(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[1]) {or (nMax - nMin > 1500)};
end;

function IsPersentSpc(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[3]) {or (nMax - nMin > 6000)};
end;

function IsPersentBook(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[5]) {or (nMax - nMin > 6000)};
end;

function IsPersentHPHero(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[7]) {or (nMax - nMin > 1500)};
end;

function IsPersentMPHero(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[8]) {or (nMax - nMin > 1500)};
end;

function IsPersentSpcHero(nMin, nMax: Integer): Boolean;
begin
  Result := False;
  if nMax <> 0 then
    Result := (Round((nMin / nMax) * 100) < g_gnProtectPercent[9]) {or (nMax - nMin > 6000)};
end;

procedure LoadWMImagesLib();
var
  i: Integer;
begin
  g_wui :=CreateImages('Data\ui1.data', g_GameDevice);
  g_opui :=CreateImages('Data\NewopUI.data', g_GameDevice);
  g_WMainImages :=CreateImages(MAINIMAGEFILE, g_GameDevice);
  g_WMain2Images :=CreateImages(MAINIMAGEFILE2, g_GameDevice);
  g_WMain3Images :=CreateImages(MAINIMAGEFILE3, g_GameDevice);
  g_WChrSelImages :=CreateImages(CHRSELIMAGEFILE, g_GameDevice);
 // g_WBooksImages := CreateImages('Data\books.wzl', g_GameDevice);
 // g_WMainUibImages := CreateImages('Data\gui.data', g_GameDevice);
 // g_WMiniMapImages := CreateImages('Data\minimap.wzl', g_GameDevice);

  g_WMonImg :=CreateImages('Data\Mon1.data', g_GameDevice);
  g_WMon2Img :=CreateImages('Data\Mon2.data', g_GameDevice);
  g_WMon3Img :=CreateImages('Data\Mon3.data', g_GameDevice);
  g_WMon4Img :=CreateImages('Data\Mon4.data', g_GameDevice);
  g_WMon5Img :=CreateImages('Data\Mon5.data', g_GameDevice);
  g_WMon6Img :=CreateImages('Data\Mon6.data', g_GameDevice);
  g_WMon7Img :=CreateImages('Data\Mon7.data', g_GameDevice);
  g_WMon8Img :=CreateImages('Data\Mon8.data', g_GameDevice);
  g_WMon9Img :=CreateImages('Data\Mon9.data', g_GameDevice);
  g_WMon10Img :=CreateImages('Data\Mon10.data', g_GameDevice);
  g_WMon11Img :=CreateImages('Data\Mon11.data', g_GameDevice);
  g_WMon12Img :=CreateImages('Data\Mon12.data', g_GameDevice);
  g_WMon13Img :=CreateImages('Data\Mon13.data', g_GameDevice);
  g_WMon14Img :=CreateImages('Data\Mon14.data', g_GameDevice);
  g_WMon15Img :=CreateImages('Data\Mon15.data', g_GameDevice);
  g_WMon16Img :=CreateImages('Data\Mon16.data', g_GameDevice);
  g_WMon17Img :=CreateImages('Data\Mon17.data', g_GameDevice);
  g_WMon18Img :=CreateImages('Data\Mon18.data', g_GameDevice);
  g_WMon19Img :=CreateImages('Data\Mon19.data', g_GameDevice);
  g_WMon20Img :=CreateImages('Data\Mon20.data', g_GameDevice);
  g_WMon21Img :=CreateImages('Data\Mon21.data', g_GameDevice);
  g_WMon22Img :=CreateImages('Data\Mon22.data', g_GameDevice);
  g_WMon23Img :=CreateImages('Data\Mon23.data', g_GameDevice);
  g_WMon24Img :=CreateImages('Data\Mon24.data', g_GameDevice);
  g_WMon25Img :=CreateImages('Data\Mon25.data', g_GameDevice);
  g_WMon26Img :=CreateImages('Data\Mon26.data', g_GameDevice);
  g_WMon27Img :=CreateImages('Data\Mon27.data', g_GameDevice);
  g_WMon28Img :=CreateImages('Data\Mon28.data', g_GameDevice);
  g_WMon29Img :=CreateImages('Data\Mon29.data', g_GameDevice);
  g_WMon30Img :=CreateImages('Data\Mon30.data', g_GameDevice);
  g_WMon31Img :=CreateImages('Data\Mon31.data', g_GameDevice);
  g_WMon32Img :=CreateImages('Data\Mon32.data', g_GameDevice);
  g_WMon33Img :=CreateImages('Data\Mon33.data', g_GameDevice);
  g_WMon34Img :=CreateImages('Data\Mon34.data', g_GameDevice);
  g_WMon35Img :=CreateImages('Data\Mon35.data', g_GameDevice);
  g_WMon36Img :=CreateImages('Data\Mon36.data', g_GameDevice);
  g_WMon37Img :=CreateImages('Data\Mon37.data', g_GameDevice);
  g_WMon38Img :=CreateImages('Data\Mon38.data', g_GameDevice);
  g_WMon39Img :=CreateImages('Data\Mon39.data', g_GameDevice);
  g_WMon40Img :=CreateImages('Data\Mon40.data', g_GameDevice);
  // ???? Mon41-Mon46
  g_WMon41Img :=CreateImages('Data\Mon41.data', g_GameDevice);  // ????
  g_WMon42Img :=CreateImages('Data\Mon42.data', g_GameDevice);  // ????
  g_WMon43Img :=CreateImages('Data\Mon43.data', g_GameDevice);  // ?????
  g_WMon44Img :=CreateImages('Data\Mon44.data', g_GameDevice);  // ?????
  g_WMon45Img :=CreateImages('Data\Mon45.data', g_GameDevice);  // ????
  g_WMon46Img :=CreateImages('Data\Mon46.data', g_GameDevice);  // ????

  g_WEffectImg :=CreateImages('Data\Effect.data', g_GameDevice);
  g_WDragonImg :=CreateImages('Data\Dragon.data', g_GameDevice);
  g_WSkeletonImg :=CreateImages('Data\Mon-kulou.data', g_GameDevice);

  g_cboEffect :=CreateImages(cboEffectFile, g_GameDevice);
    //g_cboEffect.m_Anti := True;
  g_cbohair :=CreateImages(cbohairFile, g_GameDevice);
  g_cbohum :=CreateImages(cbohumFile, g_GameDevice);
  g_cbohum3 :=CreateImages(cbohum3File, g_GameDevice);
  g_cboHumEffect :=CreateImages(cboHumEffectFile, g_GameDevice);
  g_cboHumEffect2 :=CreateImages(cboHumEffect2File, g_GameDevice);
  g_cboHumEffect3 :=CreateImages(cboHumEffect3File, g_GameDevice);

    //g_cboHumEffect.m_Anti := True;
  g_cboweapon :=CreateImages(cboweaponFile, g_GameDevice);
  g_cboweapon3 :=CreateImages(cboweapon3File, g_GameDevice);

  g_WMMapImages :=CreateImages(MINMAPIMAGEFILE, g_GameDevice);
    //InitWMImagesLibEx(g_WTilesImages := GetGameImagessFilePath + TITLESIMAGEFILE);
    //InitWMImagesLibEx(g_WSmTilesImages := GetGameImagessFilePath + SMLTITLESIMAGEFILE);
    //InitWMImagesLibEx(g_WTiles2Images := GetGameImagessFilePath + TITLESIMAGEFILE2);
    //InitWMImagesLibEx(g_WSmTiles2Images := GetGameImagessFilePath + SMLTITLESIMAGEFILE2);
  g_WHumWingImages :=CreateImages(HUMWINGIMAGESFILE, g_GameDevice);
  g_WHumEffect2 :=CreateImages(HUMWINGIMAGESFILE2, g_GameDevice);
  g_WHumEffect3 :=CreateImages(HUMWINGIMAGESFILE3, g_GameDevice);

  g_WBagItemImages :=CreateImages(BAGITEMIMAGESFILE, g_GameDevice);
    //if FileExists(BAGITEMIMAGESFILE2) then
  g_WBagItemImages2 :=CreateImages(BAGITEMIMAGESFILE2, g_GameDevice);
  g_WStateItemImages :=CreateImages(STATEITEMIMAGESFILE, g_GameDevice);
    //if FileExists(STATEITEMIMAGESFILE2) then
  g_WStateItemImages2 :=CreateImages(STATEITEMIMAGESFILE2, g_GameDevice);
  g_WDnItemImages :=CreateImages(DNITEMIMAGESFILE, g_GameDevice);
    //if FileExists(DNITEMIMAGESFILE2) then
  g_WDnItemImages2 :=CreateImages(DNITEMIMAGESFILE2, g_GameDevice);
    // 配饰装备图标 (戒指、靴子、腰带)
  g_WAccessoryImages :=CreateImages(ACCESSORYIMAGESFILE, g_GameDevice);
  g_WHairImgImages :=CreateImages(HAIRIMGIMAGESFILE, g_GameDevice);
  g_WHair2ImgImages :=CreateImages(HAIR2IMGIMAGESFILE, g_GameDevice);

  g_WHumImgImages :=CreateImages(HUMIMGIMAGESFILE, g_GameDevice);
    //if FileExists(HUM2IMGIMAGESFILE) then
  g_WHum2ImgImages :=CreateImages(HUM2IMGIMAGESFILE, g_GameDevice);
    //if FileExists(HUM3IMGIMAGESFILE) then
  g_WHum3ImgImages :=CreateImages(HUM3IMGIMAGESFILE, g_GameDevice);
    // 新增衣服资源 (Shape 50-63)
  g_WHum4ImgImages :=CreateImages(HUM4IMGIMAGESFILE, g_GameDevice);
    // 衣服装备特效
  g_ArmorEffectImages :=CreateImages(ARMOREFFECTFILE, g_GameDevice);

  g_WWeaponImages :=CreateImages(WEAPONIMAGESFILE, g_GameDevice);
    //if FileExists(WEAPON2IMAGESFILE) then
  g_WWeapon2Images :=CreateImages(WEAPON2IMAGESFILE, g_GameDevice);
    //if FileExists(WEAPON3IMAGESFILE) then
  g_WWeapon3Images :=CreateImages(WEAPON3IMAGESFILE, g_GameDevice);

  g_WMagIconImages :=CreateImages(MAGICONIMAGESFILE, g_GameDevice);
  g_WMagIcon2Images :=CreateImages(MAGICON2IMAGESFILE, g_GameDevice);

  g_WNpcImgImages :=CreateImages(NPCIMAGESFILE, g_GameDevice);

    //if FileExists(NPC2IMAGESFILE) then
  g_WNpc2ImgImages :=CreateImages(NPC2IMAGESFILE, g_GameDevice);

  g_WMagicImages :=CreateImages(MAGICIMAGESFILE, g_GameDevice);
    //g_WMagicImages.m_Anti := True;
  g_WMagic2Images :=CreateImages(MAGIC2IMAGESFILE, g_GameDevice);
    //g_WMagic2Images.m_Anti := True;
  g_WMagic3Images :=CreateImages(MAGIC3IMAGESFILE, g_GameDevice);
    //g_WMagic3Images.m_Anti := True;
  g_WMagic4Images :=CreateImages(MAGIC4IMAGESFILE, g_GameDevice);
    //g_WMagic4Images.m_Anti := True;
  g_WMagic5Images :=CreateImages(MAGIC5IMAGESFILE, g_GameDevice);
    //g_WMagic5Images.m_Anti := True;
  g_WMagic6Images :=CreateImages(MAGIC6IMAGESFILE, g_GameDevice);

  g_WMagic7Images :=CreateImages(MAGIC7IMAGESFILE, g_GameDevice);
  g_WMagic7Images2 :=CreateImages(MAGIC7IMAGESFILE2, g_GameDevice);

  g_WMagic8Images :=CreateImages(MAGIC8IMAGESFILE, g_GameDevice);
  g_WMagic8Images2 :=CreateImages(MAGIC8IMAGESFILE2, g_GameDevice);
  g_WMagic9Images :=CreateImages(MAGIC9IMAGESFILE, g_GameDevice);
  g_WMagic10Images :=CreateImages(MAGIC10IMAGESFILE, g_GameDevice);

  g_StateEffect :=CreateImages(STATEEFFECTFILE, g_GameDevice);
  g_ShineEffect :=CreateImages(SHINEEFFECTFILE, g_GameDevice);
  //g_Shineitem :=CreateImages(SHINEITEMFILE, g_GameDevice);
  g_ShineItems :=CreateImages(SHINEITEMSFILE, g_GameDevice);
  g_ShineDnItems :=CreateImages(SHINEDNITEMSFILE, g_GameDevice);
  //?????????
  g_WAniTilesArr :=CreateImages(ANITILESIMAGEFILE, g_GameDevice);

{$IF CUSTOMLIBFILE = 1}
  g_WEventEffectImages := CreateImages(EVENTEFFECTIMAGESFILE, g_GameDevice);
{$IFEND}
  FillChar(g_WObjectArr, SizeOf(g_WObjectArr), 0);
  FillChar(g_WTilesArr, SizeOf(g_WTilesArr), 0);
  FillChar(g_WSmTilesArr, SizeOf(g_WSmTilesArr), 0);


  //FillChar(g_WMonImagesArr, SizeOf(g_WMonImagesArr), 0);
end;

procedure InitWMImagesLibSecond;
var
  dwExitCode: DWORD;
begin
//  InitWMImagesLib(frmMain.DXDraw, False);
  //ExitThread(dwExitCode);
end;

//procedure InitWMImagesLibEx(WImages: TWMImages; DDxDraw: TDXDraw; FileName: string);
//begin
//  WImages.DXDraw := DDxDraw;
//  WImages.DDraw := DDxDraw.DDraw;
//  WImages.FileName := FileName;
//  WImages.LibType := ltUseCache;
//  WImages.Initialize;
//end;

procedure InitWMImagesLib(bFirst: Boolean);
var
  sFileName: string;
  i: Integer;
begin
  if bFirst then begin
    g_WMainImages.Initialize;
    g_WMain2Images.Initialize;
    g_WMain3Images.Initialize;
    g_WChrSelImages.Initialize;

   // g_WMainUibImages.Initialize;
   // g_WBooksImages.Initialize;
  //  g_WMiniMapImages.Initialize;

    g_opui.Initialize;
    g_wui.Initialize;
  end else begin
    g_WMonImg.Initialize;
    g_WMon2Img.Initialize;
    g_WMon3Img.Initialize;
    g_WMon4Img.Initialize;
    g_WMon5Img.Initialize;
    g_WMon6Img.Initialize;
    g_WMon7Img.Initialize;
    g_WMon8Img.Initialize;
    g_WMon9Img.Initialize;
    g_WMon10Img.Initialize;
    g_WMon11Img.Initialize;
    g_WMon12Img.Initialize;
    g_WMon13Img.Initialize;
    g_WMon14Img.Initialize;
    g_WMon15Img.Initialize;
    g_WMon16Img.Initialize;
    g_WMon16Img.Appr := 12;

    g_WMon17Img.Initialize;

    g_WMon18Img.Initialize;
    g_WMon18Img.Appr := 13;

    g_WMon19Img.Initialize;
    g_WMon19Img.Appr := 14;

    g_WMon20Img.Initialize;
    g_WMon20Img.Appr := 15;

    g_WMon21Img.Initialize;
    g_WMon21Img.Appr := 16;

    g_WMon22Img.Initialize;
    g_WMon22Img.Appr := 17;

    g_WMon23Img.Initialize;
    g_WMon23Img.Appr := 18;

    g_WMon24Img.Initialize;
    g_WMon24Img.Appr := 19;

    g_WMon25Img.Initialize;
    g_WMon25Img.Appr := 20;

    g_WMon26Img.Initialize;
    g_WMon26Img.Appr := 21;

    g_WMon27Img.Initialize;
    g_WMon27Img.Appr := 22;

    g_WMon28Img.Initialize;
    g_WMon29Img.Initialize;
    g_WMon30Img.Initialize;
    g_WMon31Img.Initialize;
    g_WMon32Img.Initialize;
    g_WMon33Img.Initialize;
    g_WMon34Img.Initialize;
    g_WMon35Img.Initialize;
    g_WMon36Img.Initialize;
    g_WMon37Img.Initialize;
    g_WMon38Img.Initialize;
    g_WMon39Img.Initialize;
    g_WMon40Img.Initialize;
    // ???? Mon41-Mon46 ???
    g_WMon41Img.Initialize;
    g_WMon42Img.Initialize;
    g_WMon43Img.Initialize;
    g_WMon44Img.Initialize;
    g_WMon45Img.Initialize;
    g_WMon46Img.Initialize;

    g_WEffectImg.Initialize;
    g_WEffectImg.Appr := 1;
    g_WDragonImg.Initialize;
    g_WSkeletonImg.Initialize;

    g_cboEffect.Initialize;
    g_cboEffect.Appr := 2;

    g_cbohair.Initialize;
    g_cbohum.Initialize;
    g_cbohum3.Initialize;

    g_cboHumEffect.Initialize;
    g_cboHumEffect.Appr := 3;
    g_cboHumEffect2.Initialize;
    g_cboHumEffect3.Initialize;

    g_cboweapon.Initialize;
    g_cboweapon3.Initialize;

    g_WMMapImages.Initialize;
    //g_WTilesImages.Initialize;
    //g_WSmTilesImages.Initialize;
    //g_WTiles2Images.Initialize;
    //g_WSmTiles2Images.Initialize;
    g_WHumWingImages.Initialize;
    g_WHumWingImages.Appr := 4;
    g_WHumEffect2.Initialize;
    g_WHumEffect2.Appr := 11;
    g_WHumEffect3.Initialize;

    g_WBagItemImages.Initialize;
    g_WStateItemImages.Initialize;
    g_WDnItemImages.Initialize;
    g_WBagItemImages2.Initialize;
    g_WStateItemImages2.Initialize;
    g_WDnItemImages2.Initialize;
    if g_WAccessoryImages <> nil then g_WAccessoryImages.Initialize;  // 配饰图标

    g_WHumImgImages.Initialize;
    g_WHum2ImgImages.Initialize;
    g_WHum3ImgImages.Initialize;
    if g_WHum4ImgImages <> nil then g_WHum4ImgImages.Initialize;  // 新增衣服
    if g_ArmorEffectImages <> nil then g_ArmorEffectImages.Initialize;  // 衣服特效
    g_WHairImgImages.Initialize;
    g_WHair2ImgImages.Initialize;
    g_WWeaponImages.Initialize;
    g_WWeapon2Images.Initialize;
    g_WWeapon3Images.Initialize;
    g_WMagIconImages.Initialize;
    g_WMagIcon2Images.Initialize;
    g_WNpcImgImages.Initialize;
    g_WNpc2ImgImages.Initialize;

    g_WMagicImages.Initialize;
    g_WMagicImages.Appr := 5;
    g_WMagic2Images.Initialize;
    g_WMagic2Images.Appr := 6;
    g_WMagic3Images.Initialize;
    g_WMagic3Images.Appr := 7;
    g_WMagic4Images.Initialize;
    g_WMagic4Images.Appr := 8;
    g_WMagic5Images.Initialize;
    g_WMagic5Images.Appr := 9;
    g_WMagic6Images.Initialize;
    g_WMagic6Images.Appr := 10;

    g_WMagic7Images.Initialize;
    g_WMagic7Images2.Initialize;
    g_WMagic8Images.Initialize;
    g_WMagic8Images2.Initialize;
    g_WMagic9Images.Initialize;
    g_WMagic10Images.Initialize;

    g_StateEffect.Initialize;
    g_StateEffect.Appr := 25;
    g_ShineEffect.Initialize;
    //g_Shineitem.Initialize;
    g_ShineItems.Initialize;
    g_ShineDnItems.Initialize;
    //??????
    g_WAniTilesArr.Initialize;

{$IF CUSTOMLIBFILE = 1}
    g_WEventEffectImages.Initialize;
{$IFEND}
  end;
end;

procedure UnLoadWMImagesLib();
var
  i: Integer;
begin
  for i := Low(g_WTilesArr) to High(g_WTilesArr) do begin
    if g_WTilesArr[i] <> nil then begin
      g_WTilesArr[i].Finalize;
      g_WTilesArr[i].Free;
    end;
  end;

  for i := Low(g_WSmTilesArr) to High(g_WSmTilesArr) do begin
    if g_WSmTilesArr[i] <> nil then begin
      g_WSmTilesArr[i].Finalize;
      g_WSmTilesArr[i].Free;
    end;
  end;

  for i := Low(g_WObjectArr) to High(g_WObjectArr) do begin
    if g_WObjectArr[i] <> nil then begin
      g_WObjectArr[i].Finalize;
      g_WObjectArr[i].Free;
    end;
  end;
  {for i := Low(g_WMonImagesArr) to High(g_WMonImagesArr) do begin
    if g_WMonImagesArr[i] <> nil then begin
      g_WMonImagesArr[i].Finalize;
      g_WMonImagesArr[i].Free;
    end;
  end;}
  try
    g_WMainImages.Finalize;
    g_WMainImages.Free;
    g_WMain2Images.Finalize;
    g_WMain2Images.Free;
    g_WMain3Images.Finalize;
    g_WMain3Images.Free;
    g_WChrSelImages.Finalize;
    g_WChrSelImages.Free;

    g_opui.Finalize;
    g_opui.Free;
    g_wui.Finalize;
    g_wui.Free;
    g_WMonImg.Finalize;
    g_WMonImg.Free;
    g_WMon2Img.Finalize;
    g_WMon2Img.Free;
    g_WMon3Img.Finalize;
    g_WMon3Img.Free;
    g_WMon4Img.Finalize;
    g_WMon4Img.Free;
    g_WMon5Img.Finalize;
    g_WMon5Img.Free;
    g_WMon6Img.Finalize;
    g_WMon6Img.Free;
    g_WMon7Img.Finalize;
    g_WMon7Img.Free;
    g_WMon8Img.Finalize;
    g_WMon8Img.Free;
    g_WMon9Img.Finalize;
    g_WMon9Img.Free;
    g_WMon10Img.Finalize;
    g_WMon10Img.Free;
    g_WMon11Img.Finalize;
    g_WMon11Img.Free;
    g_WMon12Img.Finalize;
    g_WMon12Img.Free;
    g_WMon13Img.Finalize;
    g_WMon13Img.Free;
    g_WMon14Img.Finalize;
    g_WMon14Img.Free;
    g_WMon15Img.Finalize;
    g_WMon15Img.Free;
    g_WMon16Img.Finalize;
    g_WMon16Img.Free;
    g_WMon17Img.Finalize;
    g_WMon17Img.Free;
    g_WMon18Img.Finalize;
    g_WMon18Img.Free;
    g_WMon19Img.Finalize;
    g_WMon19Img.Free;
    g_WMon20Img.Finalize;
    g_WMon20Img.Free;
    g_WMon21Img.Finalize;
    g_WMon21Img.Free;
    g_WMon22Img.Finalize;
    g_WMon22Img.Free;
    g_WMon23Img.Finalize;
    g_WMon23Img.Free;
    g_WMon24Img.Finalize;
    g_WMon24Img.Free;
    g_WMon25Img.Finalize;
    g_WMon25Img.Free;
    g_WMon26Img.Finalize;
    g_WMon26Img.Free;
    g_WMon27Img.Finalize;
    g_WMon27Img.Free;
    g_WMon28Img.Finalize;
    g_WMon28Img.Free;
    g_WMon29Img.Finalize;
    g_WMon29Img.Free;
    g_WMon30Img.Finalize;
    g_WMon30Img.Free;
    // ???? Mon41-Mon46 ??
    g_WMon41Img.Finalize;
    g_WMon41Img.Free;
    g_WMon42Img.Finalize;
    g_WMon42Img.Free;
    g_WMon43Img.Finalize;
    g_WMon43Img.Free;
    g_WMon44Img.Finalize;
    g_WMon44Img.Free;
    g_WMon45Img.Finalize;
    g_WMon45Img.Free;
    g_WMon46Img.Finalize;
    g_WMon46Img.Free;

    g_WEffectImg.Finalize;
    g_WEffectImg.Free;
    g_WDragonImg.Finalize;
    g_WDragonImg.Free;
    g_WSkeletonImg.Finalize;
    g_WSkeletonImg.Free;

    g_cboEffect.Finalize;
    g_cbohair.Finalize;
    g_cbohum.Finalize;
    g_cbohum3.Finalize;
    g_cboHumEffect.Finalize;
    g_cboHumEffect2.Finalize;
    g_cboHumEffect3.Finalize;
    g_cboweapon.Finalize;
    g_cboweapon3.Finalize;
    g_cboEffect.Free;
    g_cbohair.Free;
    g_cbohum.Free;
    g_cbohum3.Free;
    g_cboHumEffect.Free;
    g_cboHumEffect2.Free;
    g_cboHumEffect3.Free;
    g_cboweapon.Free;
    g_cboweapon3.Free;

    g_WMMapImages.Finalize;
    g_WMMapImages.Free;
    //g_WTilesImages.Finalize;
    //g_WTilesImages.Free;
    //g_WSmTilesImages.Finalize;
    //g_WSmTilesImages.Free;
    //g_WTiles2Images.Finalize;
    //g_WTiles2Images.Free;
    //g_WSmTiles2Images.Finalize;
    //g_WSmTiles2Images.Free;
    g_WHumWingImages.Finalize;
    g_WHumWingImages.Free;

    g_WHumEffect2.Finalize;
    g_WHumEffect2.Free;
    g_WHumEffect3.Finalize;
    g_WHumEffect3.Free;

    g_WBagItemImages.Finalize;
    g_WBagItemImages.Free;
    g_WStateItemImages.Finalize;
    g_WStateItemImages.Free;
    g_WDnItemImages.Finalize;
    g_WDnItemImages.Free;

    g_WBagItemImages2.Finalize;
    g_WBagItemImages2.Free;
    g_WStateItemImages2.Finalize;
    g_WStateItemImages2.Free;
    g_WDnItemImages2.Finalize;
    g_WDnItemImages2.Free;

    // 配饰图标
    if g_WAccessoryImages <> nil then begin
      g_WAccessoryImages.Finalize;
      g_WAccessoryImages.Free;
    end;

    g_WHumImgImages.Finalize;
    g_WHumImgImages.Free;

    if g_WHum2ImgImages <> nil then begin
      g_WHum2ImgImages.Finalize;
      g_WHum2ImgImages.Free;
    end;

    if g_WHum3ImgImages <> nil then begin
      g_WHum3ImgImages.Finalize;
      g_WHum3ImgImages.Free;
    end;

    if g_WHum4ImgImages <> nil then begin  // 新增衣服
      g_WHum4ImgImages.Finalize;
      g_WHum4ImgImages.Free;
    end;

    if g_ArmorEffectImages <> nil then begin  // 衣服特效
      g_ArmorEffectImages.Finalize;
      g_ArmorEffectImages.Free;
    end;

    g_WHairImgImages.Finalize;
    g_WHairImgImages.Free;

    g_WHair2ImgImages.Finalize;
    g_WHair2ImgImages.Free;

    g_WWeaponImages.Finalize;
    g_WWeaponImages.Free;

    if g_WWeapon2Images <> nil then begin
      g_WWeapon2Images.Finalize;
      g_WWeapon2Images.Free;
    end;

    if g_WWeapon3Images <> nil then begin
      g_WWeapon3Images.Finalize;
      g_WWeapon3Images.Free;
    end;

    g_WMagIconImages.Finalize;
    g_WMagIconImages.Free;
    g_WMagIcon2Images.Finalize;
    g_WMagIcon2Images.Free;

    g_WNpcImgImages.Finalize;
    g_WNpcImgImages.Free;

    g_WNpc2ImgImages.Finalize;
    g_WNpc2ImgImages.Free;

    g_WMagicImages.Finalize;
    g_WMagicImages.Free;
    g_WMagic2Images.Finalize;
    g_WMagic2Images.Free;
    g_WMagic3Images.Finalize;
    g_WMagic3Images.Free;
    g_WMagic4Images.Finalize;
    g_WMagic4Images.Free;
    g_WMagic5Images.Finalize;
    g_WMagic5Images.Free;
    g_WMagic6Images.Finalize;
    g_WMagic6Images.Free;
    g_WMagic7Images.Finalize;
    g_WMagic7Images.Free;
    g_WMagic7Images2.Finalize;
    g_WMagic7Images2.Free;
    g_WMagic8Images.Finalize;
    g_WMagic8Images.Free;
    g_WMagic8Images2.Finalize;
    g_WMagic8Images2.Free;
    g_WMagic9Images.Finalize;
    g_WMagic9Images.Free;
    g_WMagic10Images.Finalize;
    g_WMagic10Images.Free;

    g_StateEffect.Finalize;
    g_StateEffect.Free;
    g_ShineEffect.Finalize;
    g_ShineEffect.Free;
    //g_Shineitem.Finalize;
    //g_Shineitem.Free;
    g_ShineItems.Finalize;
    g_ShineItems.Free;
    g_ShineDnItems.Finalize;
    g_ShineDnItems.Free;
   // g_WBooksImages.Finalize;
   // g_WBooksImages.Free;
   // g_WMainUibImages.Finalize;
   // g_WMainUibImages.Free;
   // g_WMiniMapImages.Finalize;
  //  g_WMiniMapImages.Free;

     //?????????
    g_WAniTilesArr.Finalize;
    g_WAniTilesArr.Free;

{$IF CUSTOMLIBFILE = 1}
    g_WEventEffectImages.Finalize;
    g_WEventEffectImages.Free;
{$IFEND}
  except

  end;
end;

procedure LoadLightImages();
var
  I: integer;
  Png: TPngImage;
  AResource: TResourceStream;
begin
  for I := 0 to 5 do begin
    AResource := TResourceStream.Create(HInstance, 'PngImage'+IntToStr(I), RT_RCDATA);
    g_LightImages[I] := PXL.Providers.TGraphicsDeviceProvider(g_GameDevice.Provider).CreateLockableTexture(g_GameDevice, False);

    Png := TPngImage.Create;
    try
      Png.LoadFromStream(AResource);
      g_LightImages[I].LoadFromPng(Png);
    finally
      Png.Free;
      AResource.Free;
    end;
  end;
end;

procedure UnLoadLightImages();
var
  I: integer;
begin
  for I := 0 to 5 do begin
    if g_LightImages[I] <> nil then begin
      try
        g_LightImages[I].Free;
      finally
        g_LightImages[I] := nil;
      end;
    end;
  end;
end;

function GetObjs(nUnit, nIdx: Integer): TCustomLockableTexture; //???????
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WObjectArr)..High(g_WObjectArr)]) then nUnit := 0;
  if g_WObjectArr[nUnit] = nil then begin
    if nUnit = 0 then
      sFileName := OBJECTIMAGEFILE
    else
      sFileName := Format(OBJECTIMAGEFILE1, [nUnit + 1]);

    g_WObjectArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WObjectArr[nUnit].Initialize;
  end;
  Result := g_WObjectArr[nUnit].Images[nIdx];
end;

function GetObjsEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture; //???????
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WObjectArr)..High(g_WObjectArr)]) then nUnit := 0;
  if g_WObjectArr[nUnit] = nil then begin

    if nUnit = 0 then sFileName := OBJECTIMAGEFILE
    else sFileName := Format(OBJECTIMAGEFILE1, [nUnit + 1]);

    g_WObjectArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WObjectArr[nUnit].Initialize;
  end;
  Result := g_WObjectArr[nUnit].GetCachedImage(nIdx, px, py);
end;

function GetTiles(nUnit, nIdx: Integer): TCustomLockableTexture;
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WTilesArr)..High(g_WTilesArr)]) then
    nUnit := 0;

  if g_WTilesArr[nUnit] = nil then begin
    if nUnit = 0 then
      sFileName := TITLESIMAGEFILE
    else
      sFileName := Format(TITLESIMAGEFILEX, [nUnit + 1]);

    g_WTilesArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WTilesArr[nUnit].Initialize;
  end;
  Result := g_WTilesArr[nUnit].Images[nIdx];
end;

function GetTilesEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture;
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WTilesArr)..High(g_WTilesArr)]) then
    nUnit := 0;

  if g_WTilesArr[nUnit] = nil then begin
    if nUnit = 0 then
      sFileName := TITLESIMAGEFILE
    else
      sFileName := Format(TITLESIMAGEFILEX, [nUnit + 1]);

    g_WTilesArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WTilesArr[nUnit].Initialize;
  end;
  Result := g_WTilesArr[nUnit].GetCachedImage(nIdx, px, py);
end;

function GetSmTiles(nUnit, nIdx: Integer): TCustomLockableTexture;
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WSmTilesArr)..High(g_WSmTilesArr)]) then
    nUnit := 0;

  if g_WSmTilesArr[nUnit] = nil then begin
    if nUnit = 0 then
      sFileName := SMLTITLESIMAGEFILE
    else
      sFileName := Format(SMLTITLESIMAGEFILEX, [nUnit + 1]);

    g_WSmTilesArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WSmTilesArr[nUnit].Initialize;
  end;
  Result := g_WSmTilesArr[nUnit].Images[nIdx];
end;

function GetSmTilesEx(nUnit, nIdx: Integer; var px, py: Integer): TCustomLockableTexture;
var
  sFileName: string;
begin
  Result := nil;
  if not (nUnit in [Low(g_WSmTilesArr)..High(g_WSmTilesArr)]) then
    nUnit := 0;

  if g_WSmTilesArr[nUnit] = nil then begin
    if nUnit = 0 then
      sFileName := SMLTITLESIMAGEFILE
    else
      sFileName := Format(SMLTITLESIMAGEFILEX, [nUnit + 1]);
    g_WSmTilesArr[nUnit] := CreateImages(sFileName, g_GameDevice);
    g_WSmTilesArr[nUnit].Initialize;
  end;
  Result := g_WSmTilesArr[nUnit].GetCachedImage(nIdx, px, py);
end;

function GetMonImg(nAppr: Integer): TWMImages;
var
  WMImage: TWMImages;
begin
  Result := g_WMonImg;
  if nAppr < 1000 then begin
    case (nAppr div 10) of
      0: Result := g_WMonImg;
      1: Result := g_WMon2Img;
      2: Result := g_WMon3Img;
      3: Result := g_WMon4Img;
      4: Result := g_WMon5Img;
      5: Result := g_WMon6Img;
      6: Result := g_WMon7Img;
      7: Result := g_WMon8Img;
      8: Result := g_WMon9Img;
      9: Result := g_WMon10Img;
      10: Result := g_WMon11Img;
      11: Result := g_WMon12Img;
      12: Result := g_WMon13Img;
      13: Result := g_WMon14Img;
      14: Result := g_WMon15Img;
      15: Result := g_WMon16Img;
      16: Result := g_WMon17Img;
      17: Result := g_WMon18Img;
      18: Result := g_WMon19Img;
      19: Result := g_WMon20Img;
      20: Result := g_WMon21Img;
      21: Result := g_WMon22Img;
      22: Result := g_WMon23Img;
      23: Result := g_WMon24Img;
      24: Result := g_WMon25Img;
      25: Result := g_WMon26Img;
      26: Result := g_WMon27Img;
      27: Result := g_WMon28Img;
      28: Result := g_WMon29Img;
      29: Result := g_WMon30Img;
      30: Result := g_WMon31Img;
      31: Result := g_WMon32Img;
      32: Result := g_WMon33Img;
      33: Result := g_WMon34Img;
      34: Result := g_WMon35Img;
      35: Result := g_WMon36Img;
      36: Result := g_WMon37Img;
      37: Result := g_WMon38Img;
      38: Result := g_WMon39Img;
      39: Result := g_WMon40Img;
      // ???? Mon41-Mon46 (Appr 400-459)
      40: Result := g_WMon41Img;  // ???? Appr=400-409
      41: Result := g_WMon42Img;  // ???? Appr=410-419
      42: Result := g_WMon43Img;  // ????? Appr=420-429
      43: Result := g_WMon44Img;  // ????? Appr=430-439
      44: Result := g_WMon45Img;  // ???? Appr=440-449
      45: Result := g_WMon46Img;  // ???? Appr=450-459

      70: begin
          case (nAppr mod 100) of
            0..2: Result := g_WSkeletonImg;
          else
            Result := g_WMon28Img;
          end;
        end;
      80: Result := g_WDragonImg;
      81: Result := g_WMon36Img;
      82: Result := g_WMon36Img;
      90: begin
          case nAppr of
            904: Result := g_WMon34Img;
            905: Result := g_WMon34Img;
            906: Result := g_WMon34Img;
          else
            Result := g_WEffectImg;
          end;
        end;
    end;
  end; // else Result := GetMonImgEx(nAppr);
end;

function GetMonAction(nAppr: Integer): pTMonsterAction;
var
  FileStream: TFileStream;
  sFileName: string;
  MonsterAction: TMonsterAction;
begin
  Result := nil;
  sFileName := Format(MONPMFILE, [nAppr]);
  if FileExists(sFileName) then begin
    FileStream := TFileStream.Create(sFileName, fmOpenRead or fmShareDenyNone);
    FileStream.Read(MonsterAction, SizeOf(MonsterAction));
    New(Result);
    Result^ := MonsterAction;
    FileStream.Free;
  end;
end;

//?????????
//0 ???
//1 ????
//2 ???

function GetJobName(nJob: Integer): string;
begin
  Result := '';
  case nJob of
    0: Result := g_sWarriorName;
    1: Result := g_sWizardName;
    2: Result := g_sTaoistName;
  else begin
      Result := g_sUnKnowName;
    end;
  end;
end;

function GetItemType(ItemType: TItemType): string;
begin
  case ItemType of //
    i_HPDurg: Result := '???';
    i_MPDurg: Result := '????';
    i_HPMPDurg: Result := '????';
    i_OtherDurg: Result := '??????';
    i_Weapon: Result := '????';
    i_Dress: Result := '???';
    i_Helmet: Result := '???';
    i_Necklace: Result := '????';
    i_Armring: Result := '????';
    i_Ring: Result := '???';
    i_Belt: Result := '????';
    i_Boots: Result := '???';
    i_Charm: Result := '???';
    i_Book: Result := '??????';
    i_PosionDurg: Result := '???';
    i_UseItem: Result := '?????';
    i_Scroll: Result := '????';
    i_Stone: Result := '???';
    i_Gold: Result := '???';
    i_Other: Result := '????';
  end;
end;

function GetItemShowFilter(sItemName: string): Boolean;
var
  i: Integer;
begin
  Result := g_ShowItemList.IndexOf(sItemName) > -1;
end;

procedure LoadUserConfig(sUserName: string);
var
  ShowItem: pTShowItem;
  ini, ini2: TIniFile;
  sFileName: string;
  Strings: TStringList;
  i, no: Integer;
  sItemName, sLineText: string;
  sType, sPickup, sShow, sFColor, sBColor: string;
  sn, so: string;
begin
  sFileName := '.\Config\' + g_sServerName + '.' + sUserName + '.Set';

  ini := TIniFile.Create(sFileName);
  //base
  g_gcGeneral[0] := ini.ReadBool('Basic', 'ShowActorName', g_gcGeneral[0]);
  g_gcGeneral[1] := ini.ReadBool('Basic', 'DuraWarning', g_gcGeneral[1]);
  g_gcGeneral[2] := ini.ReadBool('Basic', 'AutoAttack', g_gcGeneral[2]);
  g_gcGeneral[3] := ini.ReadBool('Basic', 'ShowExpFilter', g_gcGeneral[3]);
  g_MaxExpFilter := ini.ReadInteger('Basic', 'ShowExpFilterMax', g_MaxExpFilter);
  g_gcGeneral[4] := ini.ReadBool('Basic', 'ShowDropItems', g_gcGeneral[4]);
  g_gcGeneral[5] := ini.ReadBool('Basic', 'ShowDropItemsFilter', g_gcGeneral[5]);
  g_gcGeneral[6] := ini.ReadBool('Basic', 'ShowHumanWing', g_gcGeneral[6]);

  g_boAutoPickUp := ini.ReadBool('Basic', 'AutoPickUp', g_boAutoPickUp);
  g_gcGeneral[7] := ini.ReadBool('Basic', 'AutoPickUpFilter', g_gcGeneral[7]);
  g_boPickUpAll := ini.ReadBool('Basic', 'PickUpAllItem', g_boPickUpAll);

  g_gcGeneral[8] := ini.ReadBool('Basic', 'HideDeathBody', g_gcGeneral[8]);
  g_gcGeneral[9] := ini.ReadBool('Basic', 'AutoFixItem', g_gcGeneral[9]);
  g_gcGeneral[10] := ini.ReadBool('Basic', 'ShakeScreen', g_gcGeneral[10]);
  g_gcGeneral[13] := ini.ReadBool('Basic', 'StruckShow', g_gcGeneral[13]);
  g_gcGeneral[15] := ini.ReadBool('Basic', 'HideStruck', g_gcGeneral[15]);

  g_gcGeneral[14] := ini.ReadBool('Basic', 'CompareItems', g_gcGeneral[14]);

  //g_gcGeneral[12] := ini.ReadBool('Basic', 'DayLight', g_gcGeneral[12]);

  ini2 := TIniFile.Create('.\lscfg.ini');
  g_gcGeneral[11] := ini2.ReadBool('Setup', 'EffectSound', g_gcGeneral[11]);
  g_SndMgr.Silent := not g_gcGeneral[11];

  g_gcGeneral[12] := ini2.ReadBool('Setup', 'EffectBKGSound', g_gcGeneral[12]);
  //g_boBGSound := g_boSound;
  g_lWavMaxVol := ini2.ReadInteger('Setup', 'EffectSoundLevel', g_lWavMaxVol);
  g_SndMgr.Volume := Round(g_lWavMaxVol / 68 * 100);
  ini2.Free;

  g_HitSpeedRate := ini.ReadInteger('Basic', 'HitSpeedRate', g_HitSpeedRate);
  g_MagSpeedRate := ini.ReadInteger('Basic', 'MagSpeedRate', g_MagSpeedRate);
  g_MoveSpeedRate := ini.ReadInteger('Basic', 'MoveSpeedRate', g_MoveSpeedRate);

  //Protect
  g_gcProtect[0] := ini.ReadBool('Protect', 'RenewHPIsAuto', g_gcProtect[0]);
  g_gcProtect[1] := ini.ReadBool('Protect', 'RenewMPIsAuto', g_gcProtect[1]);
  g_gcProtect[3] := ini.ReadBool('Protect', 'RenewSpecialIsAuto', g_gcProtect[3]);
  g_gcProtect[5] := ini.ReadBool('Protect', 'RenewBookIsAuto', g_gcProtect[5]);
  g_gcProtect[7] := ini.ReadBool('Protect', 'HeroRenewHPIsAuto', g_gcProtect[7]);
  g_gcProtect[8] := ini.ReadBool('Protect', 'HeroRenewMPIsAuto', g_gcProtect[8]);
  g_gcProtect[9] := ini.ReadBool('Protect', 'HeroRenewSpecialIsAuto', g_gcProtect[9]);
  g_gcProtect[10] := ini.ReadBool('Protect', 'HeroSidestep', g_gcProtect[10]);
  g_gcProtect[11] := ini.ReadBool('Protect', 'RenewSpecialIsAuto_MP', g_gcProtect[11]);

  g_gnProtectTime[0] := ini.ReadInteger('Protect', 'RenewHPTime', g_gnProtectTime[0]);
  g_gnProtectTime[1] := ini.ReadInteger('Protect', 'RenewMPTime', g_gnProtectTime[1]);
  g_gnProtectTime[3] := ini.ReadInteger('Protect', 'RenewSpecialTime', g_gnProtectTime[3]);
  g_gnProtectTime[5] := ini.ReadInteger('Protect', 'RenewBookTime', g_gnProtectTime[5]);
  g_gnProtectTime[7] := ini.ReadInteger('Protect', 'HeroRenewHPTime', g_gnProtectTime[7]);
  g_gnProtectTime[8] := ini.ReadInteger('Protect', 'HeroRenewMPTime', g_gnProtectTime[8]);
  g_gnProtectTime[9] := ini.ReadInteger('Protect', 'HeroRenewSpecialTime', g_gnProtectTime[9]);
  g_gnProtectPercent[0] := ini.ReadInteger('Protect', 'RenewHPPercent', g_gnProtectPercent[0]);
  g_gnProtectPercent[1] := ini.ReadInteger('Protect', 'RenewMPPercent', g_gnProtectPercent[1]);
  g_gnProtectPercent[3] := ini.ReadInteger('Protect', 'RenewSpecialPercent', g_gnProtectPercent[3]);
  g_gnProtectPercent[7] := ini.ReadInteger('Protect', 'HeroRenewHPPercent', g_gnProtectPercent[7]);
  g_gnProtectPercent[8] := ini.ReadInteger('Protect', 'HeroRenewMPPercent', g_gnProtectPercent[8]);
  g_gnProtectPercent[9] := ini.ReadInteger('Protect', 'HeroRenewSpecialPercent', g_gnProtectPercent[9]);
  g_gnProtectPercent[10] := ini.ReadInteger('Protect', 'HeroPerSidestep', g_gnProtectPercent[10]);
  g_gnProtectPercent[5] := ini.ReadInteger('Protect', 'RenewBookPercent', g_gnProtectPercent[5]);
  g_gnProtectPercent[6] := ini.ReadInteger('Protect', 'RenewBookNowBookIndex', g_gnProtectPercent[6]);
  frmMain.SendClientMessage(CM_HEROSIDESTEP, MakeLong(Integer(g_gcProtect[10]), g_gnProtectPercent[10]), 0, 0, 0);

  //
  g_gcTec[0] := ini.ReadBool('Tec', 'SmartLongHit', g_gcTec[0]);
  g_gcTec[10] := ini.ReadBool('Tec', 'SmartLongHit2', g_gcTec[10]);
  g_gcTec[11] := ini.ReadBool('Tec', 'SmartSLongHit', g_gcTec[11]);
  g_gcTec[1] := ini.ReadBool('Tec', 'SmartWideHit', g_gcTec[1]);
  g_gcTec[2] := ini.ReadBool('Tec', 'SmartFireHit', g_gcTec[2]);
  g_gcTec[3] := ini.ReadBool('Tec', 'SmartPureHit', g_gcTec[3]);
  g_gcTec[4] := ini.ReadBool('Tec', 'SmartShield', g_gcTec[4]);
  g_gcTec[5] := ini.ReadBool('Tec', 'SmartShieldHero', g_gcTec[5]);
  g_gcTec[6] := ini.ReadBool('Tec', 'SmartTransparence', g_gcTec[6]);
  g_gcTec[9] := ini.ReadBool('Tec', 'SmartThunderHit', g_gcTec[9]);

  g_gcTec[7] := ini.ReadBool('AutoPractice', 'PracticeIsAuto', g_gcTec[7]);
  g_gnTecTime[8] := ini.ReadInteger('AutoPractice', 'PracticeTime', g_gnTecTime[8]);
  g_gnTecPracticeKey := ini.ReadInteger('AutoPractice', 'PracticeKey', g_gnTecPracticeKey);

  g_gcTec[12] := ini.ReadBool('Tec', 'HeroSeriesSkillFilter', g_gcTec[12]);
  g_gcTec[13] := ini.ReadBool('Tec', 'SLongHit', g_gcTec[13]);
  g_gcTec[14] := ini.ReadBool('Tec', 'SmartGoMagic', g_gcTec[14]);
  frmMain.SendClientMessage(CM_HEROSERIESSKILLCONFIG, MakeLong(Integer(g_gcTec[12]), 0), 0, 0, 0);

  //
  g_gcHotkey[0] := ini.ReadBool('Hotkey', 'UseHotkey', g_gcHotkey[0]);
  FrmDlg.DEHeroCallHero.SetOfHotKey(ini.ReadInteger('Hotkey', 'HeroCallHero', 0));
  FrmDlg.DEHeroSetAttackState.SetOfHotKey(ini.ReadInteger('Hotkey', 'HeroSetAttackState', 0));
  FrmDlg.DEHeroSetGuard.SetOfHotKey(ini.ReadInteger('Hotkey', 'HeroSetGuard', 0));
  FrmDlg.DEHeroSetTarget.SetOfHotKey(ini.ReadInteger('Hotkey', 'HeroSetTarget', 0));
  FrmDlg.DEHeroUnionHit.SetOfHotKey(ini.ReadInteger('Hotkey', 'HeroUnionHit', 0));
  FrmDlg.DESwitchAttackMode.SetOfHotKey(ini.ReadInteger('Hotkey', 'SwitchAttackMode', 0));
  FrmDlg.DESwitchMiniMap.SetOfHotKey(ini.ReadInteger('Hotkey', 'SwitchMiniMap', 0));
  FrmDlg.DxEditSSkill.SetOfHotKey(ini.ReadInteger('Hotkey', 'SerieSkill', 0));

  g_ShowItemList.LoadFromFile('.\Data\Filter.dat');

  //============================================================================
  //g_gcAss[0] := ini.ReadBool('Ass', '0', g_gcAss[0]);
  g_gcAss[1] := ini.ReadBool('Ass', '1', g_gcAss[1]);
  g_gcAss[2] := ini.ReadBool('Ass', '2', g_gcAss[2]);
  g_gcAss[3] := ini.ReadBool('Ass', '3', g_gcAss[3]);
  g_gcAss[4] := ini.ReadBool('Ass', '4', g_gcAss[4]);
  g_gcAss[5] := ini.ReadBool('Ass', '5', g_gcAss[5]);
  g_gcAss[6] := ini.ReadBool('Ass', '6', g_gcAss[6]);

  g_APPickUpList.Clear;
  g_APMobList.Clear;

  Strings := TStringList.Create;
  if g_gcAss[5] then begin
    sFileName := '.\Config\' + g_sServerName + '.' + sUserName + '.ItemFilterEx.txt';
    if FileExists(sFileName) then
      Strings.LoadFromFile(sFileName)
    else
      Strings.SaveToFile(sFileName);

    for i := 0 to Strings.Count - 1 do begin
      if (Strings[i] = '') or (Strings[i][1] = ';') then Continue;
      so := GetValidStr3(Strings[i], sn, [',', ' ', #9]);
      no := Integer(so <> '');
      g_APPickUpList.AddObject(sn, TObject(no));
    end;
  end;

  if g_gcAss[6] then begin
    sFileName := '.\Config\' + g_sServerName + '.' + sUserName + '.MonFilter.txt';
    if FileExists(sFileName) then
      Strings.LoadFromFile(sFileName)
    else
      Strings.SaveToFile(sFileName);

    for i := 0 to Strings.Count - 1 do begin
      if (Strings[i] = '') or (Strings[i][1] = ';') then Continue;
      g_APMobList.Add(Strings[i] {, nil});
    end;
  end;
  Strings.Free;

  ini.Free;

end;

procedure LoadItemDesc();
const
  fItemDesc = '.\data\ItemDesc.dat';
var
  i: Integer;
  Name, desc: string;
  ps: PString;
  temp: TStringList;
begin
  //g_ItemDesc
  if FileExists(fItemDesc) then begin
    temp := TStringList.Create;
    temp.LoadFromFile(fItemDesc);
    for i := 0 to temp.Count - 1 do begin
      if temp[i] = '' then Continue;
      desc := GetValidStr3(temp[i], Name, ['=']);
      desc := StringReplace(desc, '\', '', [rfReplaceAll]);
      New(ps);
      ps^ := desc;
      if (Name <> '') and (desc <> '') then begin
        //g_ItemDesc.Put(name, TObject(ps));
        g_ItemDesc.AddObject(Name, TObject(ps));
      end;
    end;
    temp.Free;
  end;
end;

procedure InitObj();
begin
  //frmMain.DXDraw.AutoInitialize := True;
  DlgConf.DMsgDlg.Obj := FrmDlg.DMsgDlg;
  DlgConf.DMsgDlgOk.Obj := FrmDlg.DMsgDlgOk;
  DlgConf.DMsgDlgYes.Obj := FrmDlg.DMsgDlgYes;
  DlgConf.DMsgDlgCancel.Obj := FrmDlg.DMsgDlgCancel;
  DlgConf.DMsgDlgNo.Obj := FrmDlg.DMsgDlgNo;
  DlgConf.DLogin.Obj := FrmDlg.DLogin;
  DlgConf.DLoginNew.Obj := FrmDlg.DLoginNew;
  DlgConf.DLoginOk.Obj := FrmDlg.DLoginOk;
  DlgConf.DLoginChgPw.Obj := FrmDlg.DLoginChgPw;
  DlgConf.DLoginClose.Obj := FrmDlg.DLoginClose;
  DlgConf.DSelServerDlg.Obj := FrmDlg.DSelServerDlg;
  DlgConf.DSSrvClose.Obj := FrmDlg.DSSrvClose;
  DlgConf.DSServer1.Obj := FrmDlg.DSServer1;
  DlgConf.DSServer2.Obj := FrmDlg.DSServer2;
  DlgConf.DSServer3.Obj := FrmDlg.DSServer3;
  DlgConf.DSServer4.Obj := FrmDlg.DSServer4;
  DlgConf.DSServer5.Obj := FrmDlg.DSServer5;
  DlgConf.DSServer6.Obj := FrmDlg.DSServer6;
  DlgConf.DNewAccount.Obj := FrmDlg.DNewAccount;
  DlgConf.DNewAccountOk.Obj := FrmDlg.DNewAccountOk;
  DlgConf.DNewAccountCancel.Obj := FrmDlg.DNewAccountCancel;
  DlgConf.DNewAccountClose.Obj := FrmDlg.DNewAccountClose;
  DlgConf.DChgPw.Obj := FrmDlg.DChgPw;
  DlgConf.DChgpwOk.Obj := FrmDlg.DChgpwOk;
  DlgConf.DChgpwCancel.Obj := FrmDlg.DChgpwCancel;
  DlgConf.DSelectChr.Obj := FrmDlg.DSelectChr;
  DlgConf.DBottom.Obj := FrmDlg.DBottom;
  DlgConf.DMyState.Obj := FrmDlg.DMyState;
  DlgConf.DMyBag.Obj := FrmDlg.DMyBag;
  DlgConf.DMyMagic.Obj := FrmDlg.DMyMagic;
  DlgConf.DOption.Obj := FrmDlg.DOption;
  DlgConf.DBotMiniMap.Obj := FrmDlg.DBotMiniMap;
  DlgConf.DBotTrade.Obj := FrmDlg.DBotTrade;
  DlgConf.DBotGuild.Obj := FrmDlg.DBotGuild;
  DlgConf.DBotGroup.Obj := FrmDlg.DBotGroup;
  DlgConf.DBotPlusAbil.Obj := FrmDlg.DBotPlusAbil;
  DlgConf.DBotFriend.Obj := FrmDlg.DBotFriend;
  DlgConf.DBotDare.Obj := FrmDlg.DBotDare;
  DlgConf.DBotLevelRank.Obj := FrmDlg.DBotLevelRank;

  //DlgConf.DBotMemo.Obj := FrmDlg.DBotMemo;
  //DlgConf.DBotExit.Obj := FrmDlg.DBotExit;
  //DlgConf.DBotLogout.Obj := FrmDlg.DBotLogout;
  DlgConf.DBelt1.Obj := FrmDlg.DBelt1;
  DlgConf.DBelt2.Obj := FrmDlg.DBelt2;
  DlgConf.DBelt3.Obj := FrmDlg.DBelt3;
  DlgConf.DBelt4.Obj := FrmDlg.DBelt4;
  DlgConf.DBelt5.Obj := FrmDlg.DBelt5;
  DlgConf.DBelt6.Obj := FrmDlg.DBelt6;
  DlgConf.DGold.Obj := FrmDlg.DGold;
  DlgConf.DRfineItem.Obj := FrmDlg.DRefineItem;
  DlgConf.DCloseBag.Obj := FrmDlg.DCloseBag;
  DlgConf.DMerchantDlg.Obj := FrmDlg.DMerchantDlg;
  DlgConf.DMerchantDlgClose.Obj := FrmDlg.DMerchantDlgClose;
  //DlgConf.DConfigDlg.Obj := FrmDlg.DConfigDlg;
  //DlgConf.DConfigDlgOK.Obj := FrmDlg.DConfigDlgOK;
  //DlgConf.DConfigDlgClose.Obj := FrmDlg.DConfigDlgClose;
  DlgConf.DMenuDlg.Obj := FrmDlg.DMenuDlg;
  DlgConf.DMenuPrev.Obj := FrmDlg.DMenuPrev;
  DlgConf.DMenuNext.Obj := FrmDlg.DMenuNext;
  DlgConf.DMenuBuy.Obj := FrmDlg.DMenuBuy;
  DlgConf.DMenuClose.Obj := FrmDlg.DMenuClose;
  DlgConf.DSellDlg.Obj := FrmDlg.DSellDlg;
  DlgConf.DSellDlgOk.Obj := FrmDlg.DSellDlgOk;
  DlgConf.DSellDlgClose.Obj := FrmDlg.DSellDlgClose;
  DlgConf.DSellDlgSpot.Obj := FrmDlg.DSellDlgSpot;
  DlgConf.DKeySelDlg.Obj := FrmDlg.DKeySelDlg;
  DlgConf.DKsIcon.Obj := FrmDlg.DKsIcon;
  DlgConf.DKsF1.Obj := FrmDlg.DKsF1;
  DlgConf.DKsF2.Obj := FrmDlg.DKsF2;
  DlgConf.DKsF3.Obj := FrmDlg.DKsF3;
  DlgConf.DKsF4.Obj := FrmDlg.DKsF4;
  DlgConf.DKsF5.Obj := FrmDlg.DKsF5;
  DlgConf.DKsF6.Obj := FrmDlg.DKsF6;
  DlgConf.DKsF7.Obj := FrmDlg.DKsF7;
  DlgConf.DKsF8.Obj := FrmDlg.DKsF8;
  DlgConf.DKsConF1.Obj := FrmDlg.DKsConF1;
  DlgConf.DKsConF2.Obj := FrmDlg.DKsConF2;
  DlgConf.DKsConF3.Obj := FrmDlg.DKsConF3;
  DlgConf.DKsConF4.Obj := FrmDlg.DKsConF4;
  DlgConf.DKsConF5.Obj := FrmDlg.DKsConF5;
  DlgConf.DKsConF6.Obj := FrmDlg.DKsConF6;
  DlgConf.DKsConF7.Obj := FrmDlg.DKsConF7;
  DlgConf.DKsConF8.Obj := FrmDlg.DKsConF8;
  DlgConf.DKsNone.Obj := FrmDlg.DKsNone;
  DlgConf.DKsOk.Obj := FrmDlg.DKsOk;
  DlgConf.DItemGrid.Obj := FrmDlg.DItemGrid;
end;

function GetLevelColor(iLevel: Byte): Integer;
begin
  case iLevel of
    0: Result := $00FFFFFF;
    1: Result := $004AD663;
    2: Result := $00E9A000;
    3: Result := $00FF35B1;
    4: Result := $000061EB;
    5: Result := $005CF4FF;
    15: Result := clGray;
  else
    Result := $005CF4FF;
  end;
end;

procedure LoadItemFilter();
var
  i, n: Integer;
  s, s0, s1, s2, s3, s4, fn: string;
  ls: TStringList;
  p, p2: pTCItemRule;
begin
  fn := '.\Data\lsDefaultItemFilter.txt';
  if FileExists(fn) then begin
    ls := TStringList.Create;
    ls.LoadFromFile(fn);
    for i := 0 to ls.Count - 1 do begin
      s := ls[i];
      if s = '' then Continue;

      s := GetValidStr3(s, s0, [',']);
      s := GetValidStr3(s, s1, [',']);
      s := GetValidStr3(s, s2, [',']);
      s := GetValidStr3(s, s3, [',']);
      s := GetValidStr3(s, s4, [',']);
      New(p);
      p.Name := s0;
      p.rare := s2 = '1';
      p.pick := s3 = '1';
      p.show := s4 = '1';
      g_ItemsFilter_All.Put(s0, TObject(p));

      New(p2);
      p2^ := p^;
      g_ItemsFilter_All_Def.Put(s0, TObject(p2));

      n := StrToInt(s1);
      case n of
        0: g_ItemsFilter_Dress.AddObject(s0, TObject(p));
        1: g_ItemsFilter_Weapon.AddObject(s0, TObject(p));
        2: g_ItemsFilter_Headgear.AddObject(s0, TObject(p));
        3: g_ItemsFilter_Drug.AddObject(s0, TObject(p));
      else
        g_ItemsFilter_Other.AddObject(s0, TObject(p)); //???
      end;

    end;

    ls.Free;
  end;
end;

procedure LoadItemFilter2();
var
  i, n: Integer;
  s, s0, s1, s2, s3, s4, fn: string;
  ls: TStringList;
  p, p2: pTCItemRule;
  b2, b3, b4: Boolean;
begin
  fn := '.\Config\' + g_sServerName + '.' + frmMain.m_sCharName + '.ItemFilter.txt';
  //DScreen.AddChatBoardString(fn, clWhite, clBlue);
  if FileExists(fn) then begin
    //DScreen.AddChatBoardString('1', clWhite, clBlue);
    ls := TStringList.Create;
    ls.LoadFromFile(fn);
    for i := 0 to ls.Count - 1 do begin
      s := ls[i];
      if s = '' then Continue;

      s := GetValidStr3(s, s0, [',']);
      s := GetValidStr3(s, s2, [',']);
      s := GetValidStr3(s, s3, [',']);
      s := GetValidStr3(s, s4, [',']);

      p := pTCItemRule(g_ItemsFilter_All_Def.GetValues(s0));
      if p <> nil then begin
        //DScreen.AddChatBoardString('2', clWhite, clBlue);
        b2 := s2 = '1';
        b3 := s3 = '1';
        b4 := s4 = '1';
        if (b2 <> p.rare) or (b3 <> p.pick) or (b4 <> p.show) then begin
          //DScreen.AddChatBoardString('3', clWhite, clBlue);
          p2 := pTCItemRule(g_ItemsFilter_All.GetValues(s0));
          if p2 <> nil then begin
            //DScreen.AddChatBoardString('4', clWhite, clBlue);
            p2.rare := b2;
            p2.pick := b3;
            p2.show := b4;
          end;
        end;
      end;
    end;

    ls.Free;
  end;
end;

procedure SaveItemFilter(); //???????????
var
  i, n: Integer;
  s, s0, s1, s2, s3, s4, fn: string;
  ls: TStringList;
  p, p2: pTCItemRule;
begin
  //Savebagsdat(, @g_ItemArr);
  fn := '.\Config\' + g_sServerName + '.' + frmMain.m_sCharName + '.ItemFilter.txt';
  ls := TStringList.Create;
  for i := 0 to g_ItemsFilter_All.Count - 1 do begin
    p := pTCItemRule(g_ItemsFilter_All.GetValues(g_ItemsFilter_All.Keys[i]));
    p2 := pTCItemRule(g_ItemsFilter_All_Def.GetValues(g_ItemsFilter_All_Def.Keys[i]));
    if p.Name = p2.Name then begin
      if (p.rare <> p2.rare) or
        (p.pick <> p2.pick) or
        (p.show <> p2.show) then begin

        ls.Add(Format('%s,%d,%d,%d', [
          p.Name,
            Byte(p.rare),
            Byte(p.pick),
            Byte(p.show)
            ]));

      end;
    end;
  end;
  if ls.Count > 0 then
    ls.SaveToFile(fn);
  ls.Free;
end;

function getSuiteHint(var idx: Integer; s: string; gender: Byte): pTClientSuiteItems;
var
  i: Integer;
  p: pTClientSuiteItems;
begin
  Result := nil;
  if (idx > 12) or (idx < 0) then Exit;
  for i := 0 to g_SuiteItemsList.Count - 1 do begin
    p := g_SuiteItemsList[i];
    if {((p.asSuiteName[0] = '') or (gender = p.gender)) and }(CompareText(s, p.asSuiteName[idx]) = 0) then begin  //20200720???????????????????????????
      Result := p;
      Break;
    end;
  end;
  idx := -1;
end;

function GetItemWhere(Item: TClientItem): Integer;
begin
  Result := -1;
  if Item.s.Name = '' then Exit;
  case Item.s.StdMode of
    10, 11: begin
        Result := U_DRESS;
      end;
    5, 6: begin
        Result := U_WEAPON;
      end;
    30: begin
        Result := U_RIGHTHAND;
      end;
    19, 20, 21: begin
        Result := U_NECKLACE;
      end;
    15: begin
        Result := U_HELMET;
      end;
    16: begin

      end;
    24, 26: begin
        Result := U_ARMRINGL;
      end;
    22, 23: begin
        Result := U_RINGL;
      end;
    25: begin
        Result := U_BUJUK;
      end;
    27: begin
        Result := U_BELT;
      end;
    28: begin
        Result := U_BOOTS;
      end;
    7, 29: begin
        Result := U_CHARM;
      end;
  end;
end;

const
  g_levelstring: array[1..8] of string = ('?', '??', '??', '??', '??', '??', '??', '??');

function GetEvaBaseAbil(Val: TEvaAbil): string;
begin
  Result := '';
  if Val.btValue = 0 then Exit;
  case Val.btType of
    01: Result := Format(' ???????? +%d', [Val.btValue]);
    02: Result := Format(' ??????? +%d', [Val.btValue]);
    03: Result := Format(' ???????? +%d', [Val.btValue]);
    04: Result := Format(' ??????? +%d', [Val.btValue]);
    05: Result := Format(' ??????? +%d', [Val.btValue]);

    06: Result := Format('< ??     +%d|c=clSkyBlue>', [Val.btValue]);
    07: Result := Format('< ????     +%d|c=clSkyBlue>', [Val.btValue]);
    08: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue * 10]);
    09: Result := Format('< ????     +%d|c=clSkyBlue>', [Val.btValue]);
    10: Result := Format('< ????     +%d|c=clSkyBlue>', [Val.btValue]);
    11: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]);
    12: Result := Format('< ???     +%d|c=clSkyBlue>', [Val.btValue]);
    13: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue * 10]);
    14: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue * 10]);

    //
    15: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]);
    16: Result := Format('< ????     +%d|c=clSkyBlue>', [Val.btValue]);
    17: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]);

    18: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]);
    19: Result := Format('< ???????? +%d|c=clSkyBlue>', [Val.btValue]);

    20: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);
    21: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]);
    22: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);

    23: Result := Format('< ???????? +%d|c=clSkyBlue>', [Val.btValue * 2]);
    24: Result := Format('< ??????? +%d|c=clSkyBlue>', [Val.btValue]) + '<%|c=clSkyBlue>';

    25: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]) + '<%|c=clSkyBlue>';

    26: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);
    27: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);
    28: Result := Format('< ??????   +%d|c=clWhite>', [Val.btValue]);

    29: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);
    30: Result := Format('< ?????? +%d|c=clSkyBlue>', [Val.btValue]);

    //11: Result := Format(' ??????? +%d', [val.btValue]);
    //12: Result := Format(' ??????? +%d', [val.btValue]);
  end;
end;

function GetEvaMysteryAbil(Val, val2: Byte; var cnt: Byte; Std: TClientStdItem): string;
var
  SpSkill: Byte;
begin
  cnt := 0;
  Result := '';
  Val := Val and $7F;
  if Val and $01 <> 0 then begin
    Result := Result + '< ?????????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $02 <> 0 then begin
    Result := Result + '< ????????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $04 <> 0 then begin
    Result := Result + '< ??????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $08 <> 0 then begin
    Result := Result + '< ?????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $10 <> 0 then begin
    Result := Result + '< ??????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $20 <> 0 then begin
    Result := Result + '< ?????|c=$004AD663>\';
    Inc(cnt);
  end;
  if Val and $40 <> 0 then begin
    Result := Result + '< ????????|c=$004AD663>\';
    Inc(cnt);
  end;

  {
      0: Result := $00FFFFFF;
      1: Result := $004AD663;
      2: Result := $00E9A000;
      3: Result := $00FF35B1;
      4: Result := $000061EB;
      5: Result := $005CF4FF;
  }
  if Std.StdMode in [5, 6] then begin
    if val2 and $01 <> 0 then begin
      Result := Result + '< ???????????|c=$00E9A000>\';
      Inc(cnt);
    end;
    if val2 and $02 <> 0 then begin
      Result := Result + '< ?????????|c=$00E9A000>\';
      Inc(cnt);
    end;
    if val2 and $04 <> 0 then begin
      Result := Result + '< ???????????|c=$00E9A000>\';
      Inc(cnt);
    end;
    if val2 and $08 <> 0 then begin
      Result := Result + '< ???????????|c=$00E9A000>\';
      Inc(cnt);
    end;
  end else begin
    SpSkill := 0;
    case g_ShowSuite3 of
      1: if (g_UseItems[1].s.Name <> '') and (g_UseItems[1].s.Eva.SpSkill <> 0) then
          SpSkill := g_UseItems[1].s.Eva.SpSkill;
      2: if (g_HeroUseItems[1].s.Name <> '') and (g_HeroUseItems[1].s.Eva.SpSkill <> 0) then
          SpSkill := g_UseItems[1].s.Eva.SpSkill;
      3: if (UserState1.UseItems[1].s.Name <> '') and (UserState1.UseItems[1].s.Eva.SpSkill <> 0) then
          SpSkill := g_UseItems[1].s.Eva.SpSkill;
    end;
    g_ShowSuite3 := 0;

    if (val2 and $01 <> 0) then begin
      if (SpSkill and $01 <> 0) then
        Result := Result + '< ????????Lv+1|c=$00E9A000>\'
      else
        Result := Result + '< ????????Lv+1 (?????)|c=clGray>\';
      Inc(cnt);
    end;

    if (val2 and $2 <> 0) then begin
      if (SpSkill and $2 <> 0) or (g_SuiteSpSkill > 0) then
        Result := Result + '< ??????Lv+1|c=$00E9A000>\'
      else
        Result := Result + '< ??????Lv+1 (?????)|c=clGray>\';
      Inc(cnt);
    end;

    if (val2 and $4 <> 0) then begin
      if (SpSkill and $4 <> 0) then
        Result := Result + '< ????????Lv+1|c=$00E9A000>\'
      else
        Result := Result + '< ????????Lv+1 (?????)|c=clGray>\';
      Inc(cnt);
    end;

    if (val2 and $8 <> 0) then begin
      if (SpSkill and $8 <> 0) then
        Result := Result + '< ????????Lv+1|c=$00E9A000>\'
      else
        Result := Result + '< ????????Lv+1 (?????)|c=clGray>\';
      Inc(cnt);
    end;

    g_SuiteSpSkill := 0;
  end;
end;

function GetSecretAbil(CurrMouseItem: TClientItem): Boolean;
var
  i, start: Integer;
  adv, cnt: Byte;
  s, ss: string;
begin
  Result := False;
  if not (CurrMouseItem.s.StdMode in [5, 6, 10..13, 15..24, 26..30]) then Exit;

  if CurrMouseItem.s.Eva.AdvAbilMax > 0 then begin
    cnt := 0;
    for i := CurrMouseItem.s.Eva.BaseMax to 7 do begin
      if CurrMouseItem.s.Eva.Abil[i].btValue = 0 then
        Break;
      s := GetEvaBaseAbil(CurrMouseItem.s.Eva.Abil[i]);
      if s <> '' then begin
        Inc(cnt);
      end;
    end;

    adv := 0;
    if (CurrMouseItem.s.Eva.AdvAbil <> 0) or (CurrMouseItem.s.Eva.SpSkill <> 0) then
      GetEvaMysteryAbil(CurrMouseItem.s.Eva.AdvAbil, CurrMouseItem.s.Eva.SpSkill, adv, CurrMouseItem.s);

    cnt := cnt + adv;
    start := Integer(CurrMouseItem.s.Eva.AdvAbilMax - cnt);
    if start > 0 then
      Result := True;
  end;
end;

function GetEvaluationInfo(CurrMouseItem: TClientItem): string;
var
  i, start: Integer;
  adv, cnt: Byte;
  s, ss: string;
begin
  Result := '';

  if (CurrMouseItem.s.Eva.EvaTimesMax <> 0) and (CurrMouseItem.s.Eva.EvaTimes = 0) then begin
    Result := Result + '<?????|c=clyellow>\';
  end else begin
    if CurrMouseItem.s.Eva.EvaTimes in [1..8] then begin
      if CurrMouseItem.s.Eva.EvaTimes < CurrMouseItem.s.Eva.EvaTimesMax then
        Result := Result + Format('<%s??(??????)|c=clyellow>\', [g_levelstring[CurrMouseItem.s.Eva.EvaTimes]])
      else
        Result := Result + Format('<%s??|c=clyellow>\', [g_levelstring[CurrMouseItem.s.Eva.EvaTimes]]);
    end;
  end;

  start := 0;
  case CurrMouseItem.s.Eva.Quality of
    001..050: start := 1;
    051..100: start := 2;
    101..150: start := 3;
    151..200: start := 4;
    201..255: start := 5;
  end;
  ss := '';
  if start > 0 then begin
    for i := 0 to start - 1 do
      ss := ss + '<u~|I=1345>';
  end;
  if (ss <> '') then
    Result := Result + ' ' + ss + '\';

  //????????
  ss := '';
  if CurrMouseItem.s.Eva.BaseMax in [1..8] then begin
    for i := 0 to CurrMouseItem.s.Eva.BaseMax - 1 do begin
      s := GetEvaBaseAbil(CurrMouseItem.s.Eva.Abil[i]);
      if s <> '' then
        ss := ss + s + '\';
    end;
  end;
  if ss <> '' then
    Result := Result + ' \<???????????|c=clYellow>\' + ss;

  //????????
  ss := '';
  if CurrMouseItem.s.Eva.AdvAbilMax > 0 then begin
    cnt := 0;
    for i := CurrMouseItem.s.Eva.BaseMax to 7 do begin
      if CurrMouseItem.s.Eva.Abil[i].btValue = 0 then
        Break;
      s := GetEvaBaseAbil(CurrMouseItem.s.Eva.Abil[i]);
      if s <> '' then begin
        ss := ss + s + '\';
        Inc(cnt);
      end;
    end;

    adv := 0;
    if (CurrMouseItem.s.Eva.AdvAbil <> 0) or (CurrMouseItem.s.Eva.SpSkill <> 0) then begin
      ss := ss + GetEvaMysteryAbil(CurrMouseItem.s.Eva.AdvAbil, CurrMouseItem.s.Eva.SpSkill, adv, CurrMouseItem.s);
    end;

    cnt := cnt + adv;

    start := Integer(CurrMouseItem.s.Eva.AdvAbilMax - cnt);
    if start > 0 then
      for i := 0 to start - 1 do
        ss := ss + '< ????????(?????)|c=clRed>\';
  end;
  if ss <> '' then
    Result := Result + ' \<????????????|c=clYellow>\' + ss;

  //???
  ss := '';
  if CurrMouseItem.s.Eva.SpiritMax > 0 then begin
    if CurrMouseItem.s.Eva.Spirit > 0 then
      ss := ss + Format('<???????|c=clLime> <???%d|c=clYellow> <????%d/%d|c=clYellow>\', [CurrMouseItem.s.Eva.SpiritQ, CurrMouseItem.s.Eva.Spirit, CurrMouseItem.s.Eva.SpiritMax])
    else
      ss := ss + Format('<???????|c=clLime> <???%d|c=clYellow> <????0/%d|c=clRed>\', [CurrMouseItem.s.Eva.SpiritQ, CurrMouseItem.s.Eva.SpiritMax]);
  end;
  if ss <> '' then
    Result := Result + '-\' + ss;

end;

procedure InitClientItems();
begin
  FillChar(g_MagicArr, SizeOf(g_MagicArr), 0);
  FillChar(g_TakeBackItemWait, SizeOf(g_TakeBackItemWait), 0);

  FillChar(g_UseItems, SizeOf(TClientItem) * 14, #0);
  FillChar(g_ItemArr, SizeOf(TClientItem) * MAXBAGITEMCL, #0);
  FillChar(g_HeroUseItems, SizeOf(TClientItem) * 14, #0);
  FillChar(g_HeroItemArr, SizeOf(TClientItem) * MAXBAGITEMCL, #0);
  FillChar(g_RefineItems, SizeOf(TMovingItem) * 3, #0);
  FillChar(g_BuildAcuses, SizeOf(g_BuildAcuses), #0);
  FillChar(g_DetectItem, SizeOf(g_DetectItem), #0);
  FillChar(g_TIItems, SizeOf(g_TIItems), #0);
  FillChar(g_spItems, SizeOf(g_spItems), #0);

  FillChar(g_ItemArr, SizeOf(TClientItem) * MAXBAGITEMCL, #0);
  FillChar(g_HeroItemArr, SizeOf(TClientItem) * MAXBAGITEMCL, #0);

  FillChar(g_DealItems, SizeOf(TClientItem) * 10, #0);
  FillChar(g_YbDealItems, SizeOf(TClientItem) * 10, #0);
  FillChar(g_DealRemoteItems, SizeOf(TClientItem) * 20, #0);
end;

function GetTIHintString1(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
begin
  Result := 0;
  g_tiHintStr1 := '';
  case idx of
    0: begin
        g_tiHintStr1 := '????????????????????????????????????????????????????????????????????????????';
        FrmDlg.DBTIbtn1.OnbtnState := tdisable;
        FrmDlg.DBTIbtn1.Caption := '???????';
        FrmDlg.DBTIbtn2.OnbtnState := tdisable;
        FrmDlg.DBTIbtn2.Caption := '???????';
      end;
    1: begin
        if (ci = nil) or (ci.s.Name = '') then Exit;
        if ci.s.Eva.EvaTimesMax = 0 then begin
          g_tiHintStr1 := '?????????????????????????????????????';
          FrmDlg.DBTIbtn1.OnbtnState := tdisable;
          FrmDlg.DBTIbtn1.Caption := '???????';
          FrmDlg.DBTIbtn2.OnbtnState := tdisable;
          FrmDlg.DBTIbtn2.Caption := '???????';
          Exit;
        end;
        if ci.s.Eva.EvaTimes < ci.s.Eva.EvaTimesMax then begin
          if FrmDlg.DWTI.tag = 1 then begin
            case ci.s.Eva.EvaTimes of
              0: begin
                  g_tiHintStr1 := '????????????????????????????????????????';
                  FrmDlg.DBTIbtn1.OnbtnState := tnor;
                  FrmDlg.DBTIbtn1.Caption := '??????';
                  FrmDlg.DBTIbtn2.OnbtnState := tnor;
                  FrmDlg.DBTIbtn2.Caption := '??????';
                end;
              1: begin
                  g_tiHintStr1 := '?????????????????????????????????????????';
                  FrmDlg.DBTIbtn1.OnbtnState := tnor;
                  FrmDlg.DBTIbtn1.Caption := '???????';
                  FrmDlg.DBTIbtn2.OnbtnState := tnor;
                  FrmDlg.DBTIbtn2.Caption := '???????';
                end;
              2: begin
                  g_tiHintStr1 := '??????????????????????????????????????????';
                  FrmDlg.DBTIbtn1.OnbtnState := tnor;
                  FrmDlg.DBTIbtn1.Caption := '???????';
                  FrmDlg.DBTIbtn2.OnbtnState := tnor;
                  FrmDlg.DBTIbtn2.Caption := '???????';
                end;
            else begin
                g_tiHintStr1 := '????????????????????????????????????';
                FrmDlg.DBTIbtn1.OnbtnState := tnor;
                FrmDlg.DBTIbtn1.Caption := '???????';
                FrmDlg.DBTIbtn2.OnbtnState := tnor;
                FrmDlg.DBTIbtn2.Caption := '???????';
              end;
            end;
          end else if FrmDlg.DWTI.tag = 2 then begin
            FrmDlg.DBTIbtn1.OnbtnState := tnor;
            FrmDlg.DBTIbtn1.Caption := '????';
          end;

          Result := ci.s.Eva.EvaTimes;
        end else begin
          g_tiHintStr1 := Format('??????%s???????????????', [ci.s.Name]);
          FrmDlg.DBTIbtn1.OnbtnState := tdisable;
          FrmDlg.DBTIbtn1.Caption := '???????';
          FrmDlg.DBTIbtn2.OnbtnState := tdisable;
          FrmDlg.DBTIbtn2.Caption := '???????';
        end;
      end;

    2: g_tiHintStr1 := Format('????????????????????????????????%s??????', [iname]);
    3: g_tiHintStr1 := Format('????????????????????????????????%s??????????', [iname]);
    4: g_tiHintStr1 := Format('??%s??????????????????????????????????????????????????', [iname]);
    5: g_tiHintStr1 := Format('?????????????%s????????????????????????????????????', [iname]);
    6: g_tiHintStr1 := Format('?????????????%s?????????????', [iname]);
    7: g_tiHintStr1 := Format('?????????????%s???????????????????????????????', [iname]);

    8: g_tiHintStr1 := '??????????????';
    9: g_tiHintStr1 := Format('?????????????????????????????%s??', [iname]);
    10: g_tiHintStr1 := '?????????????????';
    11: g_tiHintStr1 := Format('??????%s???????????', [iname]);
    12: begin
        FrmDlg.DBTIbtn1.OnbtnState := tdisable;
        FrmDlg.DBTIbtn2.OnbtnState := tdisable;
        g_tiHintStr1 := Format('??????????????%s?????????????????', [iname]);
      end;

    30: g_tiHintStr1 := '?????????????????';
    31: g_tiHintStr1 := Format('????????%s???????????????????????', [iname]);

    32: g_tiHintStr1 := Format('??????????????%s??????', [iname]);
    33: g_tiHintStr1 := Format('?????????%s??????????????????', [iname]);
  end;
end;

function GetTIHintString2(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
begin
  Result := 0;
  g_tiHintStr1 := '';
  case idx of
    0: begin
        g_tiHintStr1 := '?????????????????????????????????????????????????????????????????????????????????????????';
        FrmDlg.DBTIbtn1.OnbtnState := tdisable;
      end;
    1: begin
        g_tiHintStr1 := Format('???%s??????????????????????????????????????%s????????????????????????????????????', [ci.s.Name, ci.s.Name]);
        FrmDlg.DBTIbtn1.OnbtnState := tnor;
      end;
    2: g_tiHintStr1 := Format('???????????????????????%s???????????%s???????????????????', [iname, iname]);
    3: g_tiHintStr1 := '???????????';
    4: g_tiHintStr1 := Format('??????%s????????????', [iname]);
    5: g_tiHintStr1 := '??????????????????????';
    6: g_tiHintStr1 := '???????????????????????????????????????????????????????';
    7: g_tiHintStr1 := '????????????????????????????????????????????????????';
    8: g_tiHintStr1 := '???????????';
  end;
end;

function GetSpHintString1(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
begin
  Result := 0;
  g_spHintStr1 := '';
  case idx of
    0: g_spHintStr1 := '??????????????????????????????????????????????????????????';
    1: g_spHintStr1 :=
      '??????????????????????????????' +
        '?????????????????????????????' +
        '????????????????';
    2: g_spHintStr1 := '?????????????????';
    3: begin
        FrmDlg.DBSP.OnbtnState := tdisable;
        g_spHintStr1 := '?????????????????';
      end;
    4: g_spHintStr1 := '??????????????????';
    5: g_spHintStr1 := '?????????????';
    6: g_spHintStr1 := '????????????';
    7: g_spHintStr1 := '?????????????????????????????????????????';

    10: g_spHintStr1 := '????????????????';
    11: g_spHintStr1 :=
      '????????????????????????????????' +
        '?????????????????????????????????' +
        '???????????';
    12: g_spHintStr1 := '????????????';
    13: g_spHintStr1 := '????????????';
    14: g_spHintStr1 := '???????????';
    15: g_spHintStr1 := '??????????????????';

  end;
end;

function GetSpHintString2(idx: Integer; ci: PTClientItem = nil; iname: string = ''): Byte;
begin
  Result := 0;
  g_spHintStr1 := '';
  case idx of
    0: begin
        g_spHintStr1 := '?????????????????????????????????????????????????????????????????????????????';
      end;
  end;
end;

procedure AutoPutOntiBooks();
var
  i: Integer;
  cu: TClientItem;
begin
  if FrmDlg.DWTI.Visible and (FrmDlg.DWTI.tag = 1) and
    (g_TIItems[0].Item.s.Name <> '') and
    (g_TIItems[0].Item.s.Eva.EvaTimesMax > 0) and
    (g_TIItems[0].Item.s.Eva.EvaTimes < g_TIItems[0].Item.s.Eva.EvaTimesMax) and
    ((g_TIItems[1].Item.s.Name = '') or
    (g_TIItems[1].Item.s.StdMode <> 56) or
    not (g_TIItems[1].Item.s.Shape in [1..3]) or
    (g_TIItems[1].Item.s.Shape <> g_TIItems[0].Item.s.Eva.EvaTimes + 1)) then begin
    for i := MAXBAGITEMCL - 1 downto 6 do begin
      if (g_ItemArr[i].s.Name <> '') and (g_ItemArr[i].s.StdMode = 56) and (g_ItemArr[i].s.Shape = g_TIItems[0].Item.s.Eva.EvaTimes + 1) then begin
        if g_TIItems[1].Item.s.Name <> '' then begin
          cu := g_TIItems[1].Item;
          g_TIItems[1].Item := g_ItemArr[i];
          g_TIItems[1].Index := i;
          g_ItemArr[i] := cu;
        end else begin
          g_TIItems[1].Item := g_ItemArr[i];
          g_TIItems[1].Index := i;
          g_ItemArr[i].s.Name := '';
        end;
        Break;
      end;
    end;
  end;
end;

procedure AutoPutOntiSecretBooks();
var
  i: Integer;
  cu: TClientItem;
begin
  if FrmDlg.DWSP.Visible and (FrmDlg.DWSP.tag = 1) and
    (g_spItems[0].Item.s.Name <> '') and
    (g_spItems[0].Item.s.Eva.EvaTimesMax > 0) and
    ((g_spItems[1].Item.s.Name = '') or
    (g_spItems[1].Item.s.StdMode <> 56) or
    (g_spItems[1].Item.s.Shape <> 0)) then begin
    for i := MAXBAGITEMCL - 1 downto 6 do begin
      if (g_ItemArr[i].s.Name <> '') and (g_ItemArr[i].s.StdMode = 56) and (g_ItemArr[i].s.Shape = 0) then begin
        if g_spItems[1].Item.s.Name <> '' then begin
          cu := g_spItems[1].Item;
          g_spItems[1].Item := g_ItemArr[i];
          g_spItems[1].Index := i;
          g_ItemArr[i] := cu;
        end else begin
          g_spItems[1].Item := g_ItemArr[i];
          g_spItems[1].Index := i;
          g_ItemArr[i].s.Name := '';
        end;
        Break;
      end;
    end;
  end;
end;

procedure AutoPutOntiCharms();
var
  i, n: Integer;
  cu: TClientItem;
begin
  n := 0;
  if FrmDlg.DWTI.Visible and (FrmDlg.DWTI.tag = 2) and
    (g_TIItems[0].Item.s.Name <> '') and
    (g_TIItems[0].Item.s.Eva.EvaTimesMax > 0) and
    (g_TIItems[0].Item.s.Eva.EvaTimes > 0) and
    ((g_TIItems[1].Item.s.Name = '') or (g_TIItems[1].Item.s.StdMode <> 41) or (g_TIItems[1].Item.s.Shape <> 30)) then begin
    n := 1;
    for i := MAXBAGITEMCL - 1 downto 6 do begin
      n := 2;
      if (g_ItemArr[i].s.Name <> '') and (g_ItemArr[i].s.StdMode = 41) and (g_ItemArr[i].s.Shape = 30) then begin
        n := 3;
        if g_TIItems[1].Item.s.Name <> '' then begin
          n := 4;
          cu := g_TIItems[1].Item;
          g_TIItems[1].Item := g_ItemArr[i];
          g_TIItems[1].Index := i;
          g_ItemArr[i] := cu;
        end else begin
          n := 5;
          g_TIItems[1].Item := g_ItemArr[i];
          g_TIItems[1].Index := i;
          g_ItemArr[i].s.Name := '';
        end;
        Break;
      end;
    end;
  end;
  //DScreen.AddChatBoardString(inttostr(n), clWhite, GetRGB($FC));
end;

function GetSuiteAbil(idx, Shape: Integer; var sa: TtSuiteAbil): Boolean;
var
  i: Integer;
begin
  FillChar(sa, SizeOf(sa), 0);
  case idx of
    1: begin
        for i := Low(TtSuiteAbil) to High(TtSuiteAbil) do begin
          if (g_UseItems[i].s.Name <> '') and ((g_UseItems[i].s.Shape = Shape) or (g_UseItems[i].s.AniCount = Shape)) then
            sa[i] := 1;
        end;
      end;
    2: for i := Low(TtSuiteAbil) to High(TtSuiteAbil) do begin
        if (g_HeroUseItems[i].s.Name <> '') and ((g_HeroUseItems[i].s.Shape = Shape) or (g_HeroUseItems[i].s.AniCount = Shape)) then
          sa[i] := 1;
      end;
    3: for i := Low(TtSuiteAbil) to High(TtSuiteAbil) do begin
        if (UserState1.UseItems[i].s.Name <> '') and ((UserState1.UseItems[i].s.Shape = Shape) or (UserState1.UseItems[i].s.AniCount = Shape)) then
          sa[i] := 1;
      end;
  end;
end;

procedure ScreenChanged();
begin
  g_TileMapOffSetX := Round(SCREENWIDTH div 2 / UNITX) + 1;
  g_TileMapOffSetY := Round(SCREENHEIGHT div 2 / UNITY);
  AAX := (SCREENWIDTH - UNITX) div 2 mod UNITX;

  LMX := Round(SCREENWIDTH / UNITX * 2) + 4; //30
  LMY := Round((SCREENHEIGHT - 150) / UNITX * 2); //26;
end;

function ShiftYOffset(): Integer;
begin
  Result := Round((SCREENHEIGHT - 150) / UNITY / 2) * UNITY - HALFY;
end;

procedure InitScreenConfig();
var
  i: Integer;
  ini: TIniFile;
begin
//  ini := TIniFile.Create('.\lscfg.ini');
//  SCREENWIDTH := ini.ReadInteger('Setup', 'SCREENWIDTH', 800);
//  SCREENHEIGHT := ini.ReadInteger('Setup', 'SCREENHEIGHT', 600);
//  g_boFullScreen := ini.ReadBool('Setup', 'FullScreen', False);
////  g_Windowed := ini.ReadBool('Setup', 'Windowed', True);
//  g_VSync := ini.ReadBool('Setup', 'VSync', True);
//  ini.Free;
  SCREENWIDTH := g_MirStartupInfo.nScreenWidth;
  SCREENHEIGHT := g_MirStartupInfo.nScreenHegiht;
  g_boFullScreen := g_MirStartupInfo.boFullScreen;
  g_VSync := g_MirStartupInfo.boWaitVBlank;

  BOTTOMTOP := SCREENHEIGHT - 251;
  WINRIGHT := SCREENWIDTH - 60;
  BOTTOMEDGE := SCREENHEIGHT - 30;
  SURFACEMEMLEN := ((SCREENWIDTH + 3) div 4) * 4 * SCREENHEIGHT;
  MAPSURFACEWIDTH := SCREENWIDTH;
  MAPSURFACEHEIGHT := SCREENHEIGHT - 150;
  BOXWIDTH := (SCREENWIDTH div 2 - 214) * 2;

  with g_SkidAD_Rect do begin
    Left := 5;
    Top := 6;
    Right := SCREENWIDTH - 5;
    Bottom := 6 + 20;
  end;

  with g_SkidAD_Rect2 do begin
    Left := 183;
    Top := 6;
    Right := SCREENWIDTH - 208;
    Bottom := 6 + 20;
  end;

  with G_RC_LEVEL do begin
    Left := SCREENWIDTH - 800 + 664;
    Top := 144;
    Right := SCREENWIDTH - 800 + 664 + 34;
    Bottom := 144 + 14;
  end;
  with G_RC_EXP do begin
    Left := SCREENWIDTH - 800 + 666;
    Top := 178;
    Right := SCREENWIDTH - 800 + 666 + 72;
    Bottom := 178 + 14;
  end;
  with G_RC_WEIGTH do begin
    Left := SCREENWIDTH - 800 + 666;
    Top := 212;
    Right := SCREENWIDTH - 800 + 666 + 72;
    Bottom := 212 + 14;
  end;
  with G_RC_SQUENGINER do begin
    Left := 78;
    Top := 90;
    Right := Left + 16;
    Bottom := Top + 95;
  end;
  with G_RC_IMEMODE do begin
    Left := SCREENWIDTH - 270 - 65;
    Top := 105;
    Right := Left + 60;
    Bottom := Top + 9;
  end;
end;

function IsInMyRange(Act: TActor): Boolean;
begin
  Result := False;
  if (Act = nil) or (g_MySelf = nil) then
    Exit;
  if (abs(Act.m_nCurrX - g_MySelf.m_nCurrX) <= (g_TileMapOffSetX - 2)) and (abs(Act.m_nCurrY - g_MySelf.m_nCurrY) <= (g_TileMapOffSetY - 1)) then
    Result := True;
end;

function IsItemInMyRange(X, Y: Integer): Boolean;
begin
  Result := False;
  if (g_MySelf = nil) then
    Exit;
  if (abs(X - g_MySelf.m_nCurrX) <= _MIN(24, g_TileMapOffSetX + 9)) and (abs(Y - g_MySelf.m_nCurrY) <= _MIN(24, (g_TileMapOffSetY + 10))) then
    Result := True;
end;

function GetTitle(nItemIdx: Integer): pTClientStdItem;
begin
  Result := nil;
  Dec(nItemIdx);
  if (nItemIdx >= 0) and (g_TitlesList.Count > nItemIdx) then begin
    if pTStdItem(g_TitlesList.Items[nItemIdx]).Name <> '' then
      Result := g_TitlesList.Items[nItemIdx];
  end;
end;

initialization
  FillChar(g_WTilesArr, SizeOf(g_WTilesArr), 0);
  FillChar(g_WSmTilesArr, SizeOf(g_WSmTilesArr), 0);
  FillChar(g_WObjectArr, SizeOf(g_WObjectArr), 0);
  New(g_APPass);
  New(g_dwThreadTick);
  g_dwThreadTick^ := 0;
  New(g_pRcHeader);
  New(g_pbRecallHero);
  g_pbRecallHero^ := False;

  //InitializeCriticalSection(ProcImagesCS);
  InitializeCriticalSection(ProcMsgCS);
  InitializeCriticalSection(ThreadCS);
  g_APPickUpList := THStringList.Create;
  g_APMobList := THStringList.Create;

  g_ItemsFilter_All := TCnHashTableSmall.Create;
  g_ItemsFilter_All_Def := TCnHashTableSmall.Create;
  g_ItemsFilter_Dress := TStringList.Create;
  g_ItemsFilter_Weapon := TStringList.Create;
  g_ItemsFilter_Headgear := TStringList.Create;
  g_ItemsFilter_Drug := TStringList.Create;
  g_ItemsFilter_Other := TStringList.Create;
  //g_VCLUnZip1 := TVCLUnZip.Create(nil);
  g_SuiteItemsList := TList.Create;
  g_pshine:= TSimpleMsgPack.Create;
  g_TitlesList := TList.Create;
  g_xMapDescList := TStringList.Create;
  g_xCurMapDescList := TStringList.Create;

finalization
  Dispose(g_APPass);
  //DeleteCriticalSection(ProcImagesCS);
  DeleteCriticalSection(ProcMsgCS);
  DeleteCriticalSection(ThreadCS);
  g_APPickUpList.Free;
  g_APMobList.Free;
  g_ItemsFilter_All.Free;
  g_ItemsFilter_All_Def.Free;
  g_ItemsFilter_Dress.Free;
  g_ItemsFilter_Weapon.Free;
  g_ItemsFilter_Headgear.Free;
  g_ItemsFilter_Drug.Free;
  g_ItemsFilter_Other.Free;
  //g_VCLUnZip1.Free;
  g_SuiteItemsList.Free;
  g_pshine.Free;

  g_xMapDescList.Free;
  g_xCurMapDescList.Free;

end.




