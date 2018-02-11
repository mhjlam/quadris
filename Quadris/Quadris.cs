using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Quadris
{
	public class Constants
	{
		public const int ScreenWidth = 640;
		public const int ScreenHeight = 480;

		public const int PieceTiles = 5;
		public const int TileSize = 16;

		public const int WellWidth = 10;
		public const int WellHeight = 20;
		public const int WellCenterX = ScreenWidth / 2;
		public const int WellCenterY = ScreenHeight / 2;

		public const int WellLeft = WellCenterX - (WellWidth * TileSize / 2);
		public const int WellRight = WellCenterX + (WellWidth * TileSize / 2);
		public const int WellTop = WellCenterY - (WellHeight * TileSize / 2);
		public const int WellBottom = WellCenterY + (WellHeight * TileSize / 2);
	}

	public class Quadris : Game
	{
		GraphicsDeviceManager graphicsDevice;
		SpriteBatch spriteBatch;
		Texture2D tileSurface;

		KeyboardState prevKeyState = Keyboard.GetState();

		Random random = new Random();
		TimeSpan gravityTime = TimeSpan.FromSeconds(1.0);
		TimeSpan elapsedTime = TimeSpan.Zero;

		Well well = new Well();
		Tetromino piece = new Tetromino();
		Tetromino nextPiece = new Tetromino();

		public Quadris()
		{
			Content.RootDirectory = "Content";

			graphicsDevice = new GraphicsDeviceManager(this)
			{
				IsFullScreen = false,
				PreferredBackBufferWidth = Constants.ScreenWidth,
				PreferredBackBufferHeight = Constants.ScreenHeight
			};
		}

		protected override void Initialize()
		{
			Window.AllowUserResizing = false;

			SpawnPiece(true);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			tileSurface = new Texture2D(graphicsDevice.GraphicsDevice, 1, 1, false, SurfaceFormat.Color); // empty 1x1 color surface
		}

		protected override void UnloadContent()
		{
			tileSurface.Dispose();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			UpdateInput(gameTime);
			UpdatePiece(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			DrawScene();

			base.Draw(gameTime);
		}


		private void UpdateInput(GameTime gameTime)
		{
			KeyboardState newState = Keyboard.GetState();

			// Exit when escape key is pressed
			if (newState.IsKeyDown(Keys.Escape))
			{
				Exit();
			}
			
			if (newState.IsKeyDown(Keys.Left) && !prevKeyState.IsKeyDown(Keys.Left))
			{
				if (!well.Collision(piece.X - 1, piece.Y, piece.Tiles))
				{
					piece.X--;
				}
			}
			else if (newState.IsKeyDown(Keys.Right) && !prevKeyState.IsKeyDown(Keys.Right))
			{
				if (!well.Collision(piece.X + 1, piece.Y, piece.Tiles))
				{
					piece.X++;
				}
			}
			else if (newState.IsKeyDown(Keys.Down) && !prevKeyState.IsKeyDown(Keys.Down))
			{
				if (!well.Collision(piece.X, piece.Y + 1, piece.Tiles))
				{
					// Reset glide timer
					elapsedTime = TimeSpan.Zero;
					piece.Y++;
				}
			}

			if (newState.IsKeyDown(Keys.Z) && !prevKeyState.IsKeyDown(Keys.Z))
			{
				int[,] rotatedTiles = piece.RotateCW();

				if (!well.Collision(piece.X, piece.Y, rotatedTiles))
				{
					piece.Tiles = rotatedTiles;
				}
			}

			if (newState.IsKeyDown(Keys.X) && !prevKeyState.IsKeyDown(Keys.X))
			{
				// Drop piece
				while (!well.Collision(piece.X, piece.Y, piece.Tiles))
				{
					piece.Y++;
				}

				well.StorePiece(piece.X, piece.Y - 1, piece);
				well.ClearLines();

				elapsedTime = TimeSpan.Zero;
				SpawnPiece();
			}

			// Update keyboard state
			prevKeyState = newState;
		}

		private void UpdatePiece(GameTime gameTime)
		{
			elapsedTime += gameTime.ElapsedGameTime;

			if (elapsedTime > gravityTime)
			{
				elapsedTime = TimeSpan.Zero;

				if (!well.Collision(piece.X, piece.Y + 1, piece.Tiles))
				{
					piece.Y++;
				}
				else
				{
					well.StorePiece(piece.X, piece.Y, piece);
					well.ClearLines();

					SpawnPiece();
				}
			}
		}

		private void SpawnPiece(bool first = false)
		{
			if (first)
			{
				switch ((TetrominoType)random.Next(0, 6))
				{
					case TetrominoType.I: piece = new I(); break;
					case TetrominoType.J: piece = new J(); break;
					case TetrominoType.L: piece = new L(); break;
					case TetrominoType.O: piece = new O(); break;
					case TetrominoType.S: piece = new S(); break;
					case TetrominoType.T: piece = new T(); break;
					case TetrominoType.Z: piece = new Z(); break;
				}

				for (int i = 0; i < random.Next(0, 3); ++i)
				{
					piece.Tiles = piece.RotateCCW();
				}
			}
			else
			{
				piece = nextPiece;
			}

			piece.X = Constants.WellWidth / 2;
			piece.Y = -(Constants.PieceTiles / 2);

			// Calculate clearance space
			while (well.Collision(piece.X, piece.Y, piece.Tiles))
			{
				piece.Y++;
			}

			switch ((TetrominoType)random.Next(0, 6))
			{
				case TetrominoType.I: nextPiece = new I(); break;
				case TetrominoType.J: nextPiece = new J(); break;
				case TetrominoType.L: nextPiece = new L(); break;
				case TetrominoType.O: nextPiece = new O(); break;
				case TetrominoType.S: nextPiece = new S(); break;
				case TetrominoType.T: nextPiece = new T(); break;
				case TetrominoType.Z: nextPiece = new Z(); break;
			}

			// Generate random rotation
			for (int i = 0; i < random.Next(0, 3); ++i)
			{
				nextPiece.Tiles = nextPiece.RotateCCW();
			}

			nextPiece.X = Constants.WellWidth + 5;
			nextPiece.Y = 5;

			if (well.Collision(piece.X, piece.Y, piece.Tiles))
			{
				Exit();
			}
		}

		private void DrawScene()
		{
			DrawWell();
			DrawTetromino(piece);
			DrawTetromino(nextPiece);
		}

		private void DrawWell()
		{
			// Draw board boundaries
			DrawRectangle(Constants.WellLeft - 2, Constants.WellTop - 1, Constants.WellRight, Constants.WellBottom, Color.White, false);
			
			// Draw filled board tiles
			for (int i = 0; i < Constants.WellWidth; i++)
			{
				for (int j = 0; j < Constants.WellHeight; j++)
				{
					// Draw rectangle if tile is filled
					if (well.Tile(i, j) != 0)
					{
						DrawTile((Constants.WellLeft - 1 + i * Constants.TileSize), (Constants.WellTop - 1 + j * Constants.TileSize), well.TileColor(i, j));
					}
				}
			}
		}

		private void DrawTetromino(Tetromino tetromino)
		{
			// Position of the tile to draw
			int x = (Constants.WellCenterX - (Constants.TileSize * (Constants.WellWidth / 2))) + ((tetromino.X - Constants.PieceTiles / 2) * Constants.TileSize);
			int y = (Constants.WellCenterY - (Constants.TileSize * (Constants.WellHeight / 2))) + ((tetromino.Y - Constants.PieceTiles / 2) * Constants.TileSize);
			
			// Draw filled tiles
			for (int py = 0; py < Constants.PieceTiles; py++)
			{
				for (int px = 0; px < Constants.PieceTiles; px++)
				{
					if (tetromino.Tiles[px, py] == 0) continue;
					DrawTile((x + py * Constants.TileSize), (y + px * Constants.TileSize), tetromino.Color);
				}
			}
		}

		private void DrawTile(int left, int top, Color color)
		{
			DrawRectangle(left, top, left + Constants.TileSize - 1, top + Constants.TileSize - 1, color);
		}

		private void DrawRectangle(int left, int top, int right, int bottom, Color color, bool solid = true)
		{
			tileSurface.SetData(new Color[] { color });

			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);

			if (solid)
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, rectangle, color);
				spriteBatch.End();
			}
			else
			{
				spriteBatch.Begin();
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y, 1, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X + rectangle.Width - 1, rectangle.Y, 1, rectangle.Height), color);
				spriteBatch.Draw(tileSurface, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - 1, rectangle.Width, 1), color);
				spriteBatch.End();
			}
		}
	}


	static class Program
	{
		static void Main(string[] args)
		{
			using (Quadris quadris = new Quadris())
			{
				quadris.Run();
			}
		}
	}
}
