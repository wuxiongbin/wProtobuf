using System;
using System.Collections;

namespace wProtobuf
{
    static public class CoroutineUitlity
    {
        static public CoroutineState Run(IEnumerator ator, ICoroutine coroutine)
        {
            if (ator.MoveNext())
            {
                IYield yi = ator.Current as IYield;
                if (yi != null)
                {
                    yi.OnYield(coroutine, ator);
                    return CoroutineState.Suspend;
                }
                else if (ator.Current is IEnumerator)
                {
                    IEnumerator new_ator = ator.Current as IEnumerator;
                    coroutine.StartCoroutine(new_ator, ()=> { coroutine.StartCoroutine(ator); });
                    return CoroutineState.Suspend;
                }
                else
                {
                    return CoroutineState.Go;
                }
            }
            else
            {
                return CoroutineState.End;
            }
        }
    }
}
