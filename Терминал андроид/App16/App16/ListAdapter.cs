using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace App16
{
    class ListAdapter: BaseAdapter<ReferenceOne>
    {
        public List<ReferenceOne> Items { get; set; }
        Context mContext;

        public ListAdapter(Context cont, List<ReferenceOne> items)
        {
            Items = items;
            mContext = cont;
        }

        public override int Count
        {
            get { return Items.Count; }
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override ReferenceOne this[int position]
        {
            get { return Items[position]; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if(row==null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.item, null, false);
            }
            TextView t1 = row.FindViewById<TextView>(Resource.Id.textView1);
            TextView t2 = row.FindViewById<TextView>(Resource.Id.textView2);
            t1.Text = Items[position].Number;
            t2.Text = Items[position].Status;
            return row;
        }
    }
}