﻿/* MIT License

 * Copyright (c) 2021-2022 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using SK.Libretro.Header;
using System;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class LedHandler
    {
#if ENABLE_IL2CPP
        private static readonly retro_set_led_state_t _setLedState = SetState;
#else
        private readonly Wrapper _wrapper;
        private readonly retro_set_led_state_t _setLedState;
#endif
        private readonly ILedProcessor _processor;

        public LedHandler(Wrapper wrapper, ILedProcessor processor)
        {
#if !ENABLE_IL2CPP
            _wrapper     = wrapper;
            _setLedState = SetState;
#endif
            _processor   = processor ?? new NullLedProcessor();
        }

#if ENABLE_IL2CPP
        [MonoPInvokeCallback(typeof(retro_set_led_state_t))]
        private static void SetState(int led, int state)
        {
            if (Wrapper.TryGetInstance(System.Threading.Thread.CurrentThread, out Wrapper _wrapper))
                _wrapper.LedHandler._processor.SetState(led, state);
        }
#else
        private void SetState(int led, int state) => _wrapper.LedHandler._processor.SetState(led, state);
#endif

        public bool GetLedInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_led_interface ledInterface = data.ToStructure<retro_led_interface>();
            ledInterface.set_led_state = _setLedState.GetFunctionPointer();
            Marshal.StructureToPtr(ledInterface, data, false);
            return true;
        }
    }
}
