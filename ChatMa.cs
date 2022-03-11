using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ChatMa
{
	public class ChatMa : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte msgType = reader.ReadByte();
			if (msgType == (byte)75) {
				byte msgType2 = reader.ReadByte();
				switch (msgType2) {
					case 10: //NOMNOMNOM and Something
						int Who = reader.ReadInt32();
						NPC NOM = Main.npc[Who];
						NOM.life = 0;
						NOM.active = false;
						NOM.damage = 0;
						NetMessage.SendData(MessageID.SyncNPC, number: Who);
						break;
					case 11: //Server recieves for Copypasta
						bool which = reader.ReadBoolean();
						if (!which) { //Someone just got the copypasta
									  //Console.WriteLine("Dice roll, Send to all clients");
							ModPacket Pac = GetPacket();
							Pac.Write((byte)75); // id
							Pac.Write((byte)12); // part
							Pac.Write(false);
							Pac.Send(ignoreClient: -1);
						}
						else { //Times up
							   //Console.WriteLine("Times up, Send to all clients");
							NPC g = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
							ModPacket Pac = GetPacket();
							Pac.Write((byte)75); // id
							Pac.Write((byte)12); // part
							Pac.Write(true);
							Pac.Send(ignoreClient: -1);
							g.life = 0;
							g.HitEffect(0, 10000.0);
							g.active = false;
							NetMessage.SendData(MessageID.SyncNPC, number: g.whoAmI);

						}
						break;
					case 12: //Client recieves for Copypasta
						bool witch = reader.ReadBoolean();
						if (!witch) { //Play thisguysticks and text for all clients
							int YesGuy = NPC.FindFirstNPC(NPCID.Guide);
							Main.NewText(Main.npc[YesGuy].GivenName + " REALLY hates copypastas...", 255, 90, 50);
							Main.PlaySound(SoundLoader.customSoundType, (int)Main.npc[YesGuy].position.X, (int)Main.npc[YesGuy].position.Y, GetSoundSlot(SoundType.Custom, "Sounds/Custom/zThisGuyStinks"), 0.5f);
						}
						else { //Play guide death noise and text for all clients
							int YesGuy = NPC.FindFirstNPC(NPCID.Guide);
							Main.NewText(Main.npc[YesGuy].GivenName + " has slain himself...", 255, 90, 50);
							Main.PlaySound(SoundID.NPCDeath1, Main.npc[YesGuy].position);
							Main.npc[YesGuy].HitEffect(0, 10000.0);
						}
						break;
					case 13:
						int PWho = reader.ReadInt32();
						//Console.WriteLine("\nCane Projectile from " + Main.player[whoAmI].name);
						//Console.WriteLine("-and number " + PWho);
						//Console.WriteLine("--and owner " + Main.projectile[PWho].owner + "/" + Main.player[Main.projectile[PWho].owner].name);
						NetMessage.SendData(MessageID.SyncProjectile, number: PWho);
						break;
					default:
						break;
				}
			}
		}



		public override void Load()
		{
			if (!Main.dedServ)
			{
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/FrozenLow"), ModContent.ItemType<Items.FCaveBox>(), ModContent.TileType<Tiles.FCaveTile>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ALittleLow"), ModContent.ItemType<Items.ALittleBox>(), ModContent.TileType<Tiles.ALittleTile>());
			}
		}







		public class WeirdNPC : GlobalNPC
		{

			static bool UhOh = false;
			static int T = 0;

			public override void AI(NPC n)
			{
				if (n.netID == 22)
				{
					if (UhOh)
					{
						T++;
					}
					if (T >= 450)
					{
						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							ModPacket Pac = mod.GetPacket();
							Pac.Write((byte)75); // id
							Pac.Write((byte)11); // part
							Pac.Write(true);
							Pac.Send();
						}
						else
						{
							Main.NewText(n.GivenName + " has slain himself...", 255, 90, 50);
							//Main.PlaySound(SoundID.NPCDeath1);
							n.HitEffect(0, 10000.0);
							n.life = 0;
							n.active = false;
						}
						T = 0;
						UhOh = false;
					}
				}
			}


			public override void GetChat(NPC n, ref string chat)
			{
				if (n.netID != 22)
				{
					if (Main.rand.Next(300) < 1)
					{ // && Main.netMode != NetmodeID.MultiplayerClient
						chat = "According to all known laws of aviation, there is no way a bee should be able to fly.\nIts wings are too small to get its fat little body off the ground.\nThe bee, of course, flies anyway because bees don’t care what humans think is impossible.\nYellow, black. Yellow, black. Yellow, black. Yellow, black.\nOoh, black and yellow!\nLet’s shake it up a little.\nBarry! Breakfast is ready!\nComing!\nHang on a second.\nHello?\nBarry?\nAdam?\nCan you believe this is happening?\nI can’t.";
						int YesGuy = NPC.FindFirstNPC(NPCID.Guide);
						if (YesGuy >= 0)
						{
							UhOh = true;
							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								ModPacket Pac = mod.GetPacket();
								Pac.Write((byte)75); // id
								Pac.Write((byte)11); // part
								Pac.Write(false);
								Pac.Send();
							}
							else
							{
								Main.NewText(Main.npc[YesGuy].GivenName + " REALLY hates copypastas...", 255, 90, 50);
								Main.PlaySound(SoundLoader.customSoundType, (int)Main.npc[YesGuy].position.X, (int)Main.npc[YesGuy].position.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/zThisGuyStinks"), 0.5f);
							}
						}
					}
				}

				if (n.type == 35)
				{
					chat = "Hey buddy I know we're fighting, but I'm really strapped for cash right now. Can you help a Bone out and buy something?";
					Main.playerInventory = true;
					Main.npcChatText = "";
					Main.npcShop = Main.MaxShopIDs - 1;
					Main.instance.shop[Main.npcShop].SetupShop(n.type);
				}
			}

			public override void SetupShop(int t, Chest sh, ref int nextSlot)
			{
				if (t == 35)
				{ //Skeletron
					sh.item[nextSlot].SetDefaults(154); //Bone
					sh.item[nextSlot].shopCustomPrice = 1;
					nextSlot++;
					sh.item[nextSlot].SetDefaults(932); //Wand
					sh.item[nextSlot].shopCustomPrice = 1000;
					nextSlot++;
					sh.item[nextSlot].SetDefaults(1166); //Sword
					sh.item[nextSlot].shopCustomPrice = 400;
					nextSlot++;
					sh.item[nextSlot].SetDefaults(1320); //Pickaxe
					sh.item[nextSlot].shopCustomPrice = 300;
					nextSlot++;
					sh.item[nextSlot].SetDefaults(3451); //Banner
					sh.item[nextSlot].shopCustomPrice = 10000;
					nextSlot++;
				}

			}

			public override bool? CanChat(NPC n)
			{
				if (n.type == 35) { return true; }
				return null;
			}


			public override void NPCLoot(NPC n)
			{
				if (n.type == NPCID.Plantera)
				{
					if (Main.rand.Next(11) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.SwordCane>());
				}
				if (n.type == NPCID.Golem)
				{
					if (Main.rand.Next(7) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.SwordCane>());
				}
				if (n.type == NPCID.CultistBoss)
				{
					if (Main.rand.Next(5) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.SwordCane>());
				}
				if (n.type == NPCID.QueenBee)
				{
					if (Main.rand.Next(6) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.BeeShield>());
				}
				if (n.type == NPCID.MoonLordCore)
				{
					if (Main.rand.Next(8) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.TrueBeeShield>());
				}
				if (Main.hardMode && (n.type == NPCID.IceSlime || n.type == NPCID.IceBat || n.type == NPCID.IceTortoise || n.type == NPCID.SpikedIceSlime || n.type == NPCID.IceGolem || n.type == NPCID.IceElemental || n.type == NPCID.Penguin || n.type == NPCID.ZombieEskimo || n.type == NPCID.ArmedZombieEskimo || n.type == NPCID.PenguinBlack || n.type == NPCID.SnowFlinx || n.type == NPCID.Wolf || n.type == NPCID.IcyMerman || n.type == NPCID.ArmoredViking || n.type == NPCID.PigronHallow))
				{
					if (Main.rand.Next(300) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.FCaveBox>());
				}
				if (Main.hardMode && (n.type == NPCID.Mimic || n.type == NPCID.BigMimicCorruption || n.type == NPCID.BigMimicCrimson || n.type == NPCID.BigMimicHallow || n.type == NPCID.BigMimicJungle))
				{
					if (Main.rand.Next(9) == 0) Item.NewItem(n.getRect(), ModContent.ItemType<Items.ALittleBox>());
				}
			}
			//Skeletron head = 35
			//Bone item = 154
			//BoneWand = 932
			//BoneSword = 1166
			//BonePickaxe = 1320
			//AngryBonesBanner = 3451

		}
	}
}




//EXTRAS

/*
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			byte msgType = reader.ReadByte();
			switch (msgType)
			{
				case 75:
					byte part = reader.ReadByte();
					if (part == (byte)0 && Main.netMode == NetmodeID.Server)
					{
						for (int i = 0; i < Main.player.Length; i++)
						{
							Console.WriteLine("Sent packet to " + Main.player[i].whoAmI);
							ModPacket myPacket = GetPacket();
							myPacket.Write((byte)75); // id
							myPacket.Write((byte)1); // part
							myPacket.Send(Main.player[i].whoAmI);
						}
					}
					if (part == (byte)1)
					{
						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							int n = NPC.FindFirstNPC(NPCID.Guide);
							Main.NewText(Main.npc[n].GivenName + " REALLY hates copypastas...", 255, 90, 50);
							Main.PlaySound(SoundLoader.customSoundType, (int)Main.npc[n].position.X, (int)Main.npc[n].position.Y, GetSoundSlot(SoundType.Custom, "Sounds/Custom/zThisGuyStinks"), 0.5f);
						}
					}
					if (part == (byte)2 && Main.netMode == NetmodeID.Server) //Needs to happen on all netmodes
					{

						ModPacket myPacket = GetPacket();
						myPacket.Write((byte)75); // id
						myPacket.Write((byte)3); // part
						for (int i = 0; i < Main.player.Length; i++)
						{
							myPacket.Send(Main.player[i].whoAmI);
						}
						myPacket.Send();
					}
					if (part == (byte)3) {
						Console.WriteLine(part);
						NPC n = Main.npc[NPC.FindFirstNPC(NPCID.Guide)];
						if (Main.netMode == NetmodeID.Server) {
							n.life = 0;
							n.active = false;
						}
						else {
							Main.NewText(n.GivenName + " has slain himself...", 255, 90, 50);
							Main.PlaySound(SoundID.NPCDeath1);
							n.life = 0;
							n.active = false;
							n.HitEffect(0, 10000.0);
						}
					}
						break;
				case 76:
					break;
				default:
					Logger.WarnFormat("MyMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
		*/