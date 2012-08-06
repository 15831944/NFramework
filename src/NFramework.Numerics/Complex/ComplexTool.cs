using System;
using System.Collections.Generic;
using System.Linq;

namespace NSoft.NFramework.Numerics {
    /// <summary>
    /// ���Ҽ� ������ ���� Helper Class�Դϴ�.
    /// </summary>
    public static class ComplexTool {
        /// <summary>
        /// ������ Complex �������� ��ҵ��� modulus(length)�� ������ �ּ�, �ִ밪���� �����Ѵ�.
        /// </summary>
        public static IEnumerable<Complex> ClampLength(this IEnumerable<Complex> source, double minValue, double maxValue) {
            source.ShouldNotBeNull("source");
            if(minValue > maxValue)
                throw new InvalidOperationException("minValue greater than maxValue");

            //foreach (Complex complex in source)
            //    yield return Complex.FromModulusAndArgument(Math.Max(minValue, Math.Min(maxValue, complex.GetModulus())),
            //                                                Math.Max(minValue, Math.Min(maxValue, complex.GetArgument())));

            return
                source.Select(
                    complex => Complex.FromModulusAndArgument(Math.Max(minValue, Math.Min(maxValue, complex.GetModulus())),
                                                              Math.Max(minValue, Math.Min(maxValue, complex.GetArgument()))));
        }

        /// <summary>
        /// ������ Complex �������� ��ҵ��� ���� �ִ�, �ּҰ� �����ȿ� ��ġ ��Ų��.
        /// </summary>
        public static IEnumerable<Complex> Clamp(this IEnumerable<Complex> source, Complex min, Complex max) {
            source.ShouldNotBeNull("source");

            //foreach (Complex complex in source)
            //    yield return new Complex(Math.Min(Math.Max(complex.Re, min.Re), max.Re),
            //                             Math.Min(Math.Max(complex.Im, min.Im), max.Im));

            return
                source.Select(complex => new Complex(Math.Min(Math.Max(complex.Re, min.Re), max.Re),
                                                     Math.Min(Math.Max(complex.Im, min.Im), max.Im)));
        }

        /// <summary>
        /// ������ Complex �������� ��ҵ��� Real Part�� 0~1 ���� ������ �ϰ�, Image part�� 0�� ������ �Ѵ�.
        /// </summary>
        public static IEnumerable<Complex> ClampToRealUnit(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");

            return source.Select(
                complex => new Complex(Math.Min(Math.Max(complex.Re, 0), 1), 0));
        }

        /// <summary>
        /// ������ Offset �� ��ŭ Complex �� Shift ��Ų��.
        /// </summary>
        public static void Shift(this Complex[] source, int offset) {
            if(offset == 0)
                return;

            int count = source.Length;
            source.ShouldNotBeNull("source");
            Guard.Assert(offset >= 0 && offset < count, "offset value ranged (0 ~ [{0}]). but offset=[{1}]", count, offset);

            var temp = new Complex[count];
            for(var i = 0; i < count; i++)
                temp[(i + offset) % count] = source[i];

            for(var i = 0; i < count; i++)
                source[i] = temp[i];
        }

        /// <summary>
        /// ������ Complex ���������� �ִ�/�ּ� ���밪(Modulus/Length)�� ���Ѵ�.
        /// </summary>
        public static void GetLengthRange(this IEnumerable<Complex> source, out double min, out double max) {
            source.ShouldNotBeNull("source");

            min = double.MaxValue;
            max = double.MinValue;

            foreach(Complex complex in source) {
                double m = complex.GetModulus();
                if(m > max)
                    max = m;
                if(m < min)
                    min = m;
            }
        }

        /// <summary>
        /// ������ �ΰ��� Complex �������� ��ҵ��� ������������ ��ġ�ϴ��� �˻��Ѵ�.
        /// </summary>
        public static bool SequentialApproximateEqual(this IEnumerable<Complex> first, IEnumerable<Complex> second,
                                                      double epsilon = MathTool.Epsilon) {
            first.ShouldNotBeNull("first");
            second.ShouldNotBeNull("second");

            using(var enumerator = first.GetEnumerator())
            using(var enumerator2 = second.GetEnumerator()) {
                while(enumerator.MoveNext()) {
                    if(!enumerator2.MoveNext() || Complex.ApproximateEquals(enumerator.Current, enumerator2.Current, epsilon) == false)
                        return false;
                }
                if(enumerator2.MoveNext())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// ������ offset ��ŭ Complex �������� real part ���� ������Ų��.
        /// </summary>
        public static IEnumerable<Complex> Offset(this IEnumerable<Complex> source, double offset) {
            source.ShouldNotBeNull("source");
            return source.Select(complex => complex + offset);
        }

        /// <summary>
        /// ������ offset ��ŭ Complex �������� real part ���� ������Ų��.
        /// </summary>
        public static IEnumerable<Complex> Offset(this IEnumerable<Complex> source, Complex offset) {
            source.ShouldNotBeNull("source");
            return source.Select(complex => complex + offset);
        }

        /// <summary>
        /// �������� Complex�� scale ���� ���Ѵ�.
        /// </summary>
        public static IEnumerable<Complex> Scale(this IEnumerable<Complex> source, double scale) {
            source.ShouldNotBeNull("source");
            return source.Select(complex => complex * scale);
        }

        /// <summary>
        /// �������� Complex�� sclae ���� ���Ѵ�.
        /// </summary>
        public static IEnumerable<Complex> Scale(this IEnumerable<Complex> source, Complex scale) {
            source.ShouldNotBeNull("source");
            return source.Select(complex => complex * scale);
        }

        /// <summary>
        /// �� �������� ���մϴ�.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<Complex> Multiply(this IEnumerable<Complex> first, IEnumerable<Complex> second) {
            first.ShouldNotBeNull("first");
            second.ShouldNotBeNull("second");

            Guard.Assert(first.Count() == second.Count(), "first and second sequence length should be same.");

            using(var enumerator1 = first.GetEnumerator())
            using(var enumerator2 = second.GetEnumerator()) {
                while(enumerator1.MoveNext() && enumerator2.MoveNext()) {
                    yield return enumerator1.Current * enumerator2.Current;
                }
            }
        }

        /// <summary>
        /// ��
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<Complex> Divide(this IEnumerable<Complex> first, IEnumerable<Complex> second) {
            first.ShouldNotBeNull("first");
            second.ShouldNotBeNull("second");

            Guard.Assert(first.Count() == second.Count(), "first and second sequence length should be same.");

            using(var enumerator1 = first.GetEnumerator())
            using(var enumerator2 = second.GetEnumerator()) {
                while(enumerator1.MoveNext() && enumerator2.MoveNext()) {
                    if(enumerator2.Current != Complex.Zero)
                        yield return enumerator1.Current / enumerator2.Current;
                    else
                        yield return Complex.Zero;
                }
            }
        }

        /// <summary>
        /// <paramref name="src"/> ��Ҹ� <paramref name="dest"/>�� �����մϴ�. ��� �迭�� ũ�Ⱑ ������ ũ�⺸�� ũ�ų� ���ƾ��մϴ�.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void Copy(this Complex[] src, Complex[] dest) {
            src.ShouldNotBeNull("src");
            dest.ShouldNotBeNull("dest");
            Guard.Assert(src.Length <= dest.Length, "���� �迭�� ũ�Ⱑ ��� �迭�� ũ��� ���ų� �۾ƾ� �մϴ�.");

            Array.Copy(src, 0, dest, 0, dest.Length);
        }

        /// <summary>
        /// ������ �迭�� ���� ������ �մϴ�.
        /// </summary>
        /// <param name="source"></param>
        public static void Reverse(this Complex[] source) {
            var length = source.Length;

            for(var i = 0; i < length / 2; i++) {
                Complex temp = source[i];
                source[i] = source[length - 1 - i];
                source[length - 1 - i] = temp;
            }
        }

        /// <summary>
        /// ������ ����� Length���� 0~1�� ���� ������ ����ȭ�� �����Ѵ�.
        /// </summary>
        public static IEnumerable<Complex> Normalize(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");

            double min, max;
            source.GetLengthRange(out min, out max);

            var range = max - min;
            return source.Scale(1 / range).Offset(-min / range);
        }

        /// <summary>
        /// Invert  (1 / c)
        /// </summary>
        public static IEnumerable<Complex> Invert(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");
            return source.Select(complex => (Complex)1 / complex);
        }

        /// <summary>
        /// Complex �������� ���� ���Ѵ�.
        /// </summary>
        public static Complex Sum(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");
            return source.Aggregate(Complex.Zero, (result, next) => result + next);
        }

        /// <summary>
        /// �������� startIndex ���� count ���� ��ŭ�� ��Ҹ��� �ջ��Ѵ�.
        /// </summary>
        public static Complex SumRecursive(this IEnumerable<Complex> source, int startIndex, int count) {
            source.ShouldNotBeNull("source");

            return source.Skip(startIndex).Take(count).Sum();
        }

        /// <summary>
        /// ������ �������� Complex ����� ������ ���� ���մϴ�.
        /// </summary>
        public static Complex SumOfSquares(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");
            return source.Aggregate(Complex.Zero, (result, next) => result + next * next);
        }

        /// <summary>
        /// ������ �������� ������ �ִ� Complex ����� ������ ���� ���մϴ�.
        /// </summary>
        public static Complex SumOfSquares(this IEnumerable<Complex> source, int startIndex, int count) {
            source.ShouldNotBeNull("source");
            return source.Skip(startIndex).Take(count).SumOfSquares();
        }

        /// <summary>
        /// Returns a Norm of a value of this type, which is appropriate for measuring how
        /// close this value is to zero.
        /// </summary>
        /// <param name="complex">The <see cref="Complex"/> number to perfom this operation on.</param>
        /// <returns>A norm of this value.</returns>
        public static double Norm(this Complex complex) {
            return complex.GetModulusSquared();
        }

        /// <summary>
        /// Returns a Norm of the difference of two values of this type, which is
        /// appropriate for measuring how close together these two values are.
        /// </summary>
        /// <param name="complex">The <see cref="Complex"/> number to perfom this operation on.</param>
        /// <param name="other">The value to compare with.</param>
        /// <returns>A norm of the difference between this and the other value.</returns>
        public static double NormOfDifference(this Complex complex, Complex other) {
            return (complex - other).GetModulusSquared();
        }

        /// <summary>
        /// ���, �߰���
        /// </summary>
        public static Complex Mean(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");

            int count = source.Count();
            count.ShouldBePositive("count of source.");

            return source.Sum() / (double)count;
        }

        /// <summary>
        /// �л�
        /// </summary>
        public static Complex Variance(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");

            int count = source.Count();
            count.ShouldBePositive("count of source.");

            return source.SumOfSquares() / (double)count - source.Sum();
        }

        /// <summary>
        /// ǥ������
        /// </summary>
        public static Complex StdDev(this IEnumerable<Complex> source) {
            source.ShouldNotBeNull("source");

            return Variance(source).Sqrt();
        }

        /// <summary>
        /// ��� ������ ������ ����(Root Mean Square Error: RMS ����)
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static double RMSError(IEnumerable<Complex> alpha, IEnumerable<Complex> beta) {
            return Math.Sqrt(SumOfSquaredError(alpha, beta));
        }

        /// <summary>
        /// �� �������� ���̿� ���� ���� ���� ���� 
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static double SumOfSquaredError(IEnumerable<Complex> alpha, IEnumerable<Complex> beta) {
            double rms = 0.0;

            using(var enumerator1 = alpha.GetEnumerator())
            using(var enumerator2 = beta.GetEnumerator()) {
                while(enumerator1.MoveNext() && enumerator2.MoveNext()) {
                    var delta = enumerator2.Current - enumerator1.Current;
                    rms += delta.GetModulusSquared();
                }
            }
            return rms;
        }

        /// <summary>
        /// ������ �迭�� ������ ��ġ�� ��Ҹ� ��ȯ�մϴ�.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Swap(this Complex[] array, int left, int right) {
            var temp = array[left];
            array[left] = array[right];
            array[right] = temp;
        }

        /// <summary>
        /// �� ������ ��ȯ�մϴ�.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Swap(ref Complex a, ref Complex b) {
            var temp = a;
            a = b;
            b = temp;
        }

        private static readonly double HalfOfRoot2 = 0.5 * Math.Sqrt(2.0);

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Complex Sqrt(this Complex c) {
            double re = c.Re;
            double im = c.Im;

            double modulus = c.GetModulus();
            int sign = (im < 0) ? -1 : 1;

            c.Re = HalfOfRoot2 * Math.Sqrt(modulus + re);
            c.Im = HalfOfRoot2 * sign * Math.Sqrt(modulus - im);

            return c;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="c"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Complex Pow(this Complex c, double e) {
            double re = c.Re;
            double im = c.Im;

            double modulus = Math.Pow(c.GetModulus(), e * 0.5);
            double argument = Math.Atan2(im, re) * e;

            c.Re = modulus * Math.Cos(argument);
            c.Im = modulus * Math.Sin(argument);

            return c;
        }
    }
}