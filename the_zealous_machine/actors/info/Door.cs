using Godot;
using System;

namespace TheZealousMachine.actors.info
{
	public partial class Door : Node3D, IDoor, IConvertible
	{
		[Export]
		public bool roomSeal = false;

		[Export]
		public bool open = true;

		private bool _dirty = true;

		public override void _Ready()
		{
			_Refresh();
		}

		private void _Refresh()
		{
			MeshInstance3D meshA = GetNode<MeshInstance3D>("world_cube_corner/MeshInstance3D");
			CollisionShape3D shapeA = GetNode<CollisionShape3D>("world_cube_corner/CollisionShape3D");
			MeshInstance3D meshB = GetNode<MeshInstance3D>("world_cube_corner2/MeshInstance3D");
			CollisionShape3D shapeB = GetNode<CollisionShape3D>("world_cube_corner2/CollisionShape3D");
			bool visible = !open;
			bool disabled = open;
			meshA.Visible = visible;
			meshB.Visible = visible;
			shapeA.Disabled = disabled;
			shapeB.Disabled = disabled;
			_dirty = false;
		}

		public void SetOpen(bool flag)
		{
			open = flag;
			_dirty = true;
		}

		public override void _Process(double delta)
		{
			if (_dirty)
			{
				_Refresh();
			}
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			if (conversionType == typeof(IDoor))
			{
				return this as IDoor;
			}
			return this as object;
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
	}
}
