using System.Collections;

namespace wProtobuf
{
    public enum CoroutineState
    {
        Go, // 继续
        End, // 结束
        Suspend, // 挂起
    }

    public interface ICoroutine
    {
        CoroutineState StartCoroutine(IEnumerator routine);
        CoroutineState StartCoroutine(IEnumerator routine, Action end);
        bool StopCoroutine(IEnumerator routine);
    }

    public interface IYield
    {
        bool isDone { get; } // 是否执行完成
        void OnYield(ICoroutine coroutine, IEnumerator ator);
    }
}
